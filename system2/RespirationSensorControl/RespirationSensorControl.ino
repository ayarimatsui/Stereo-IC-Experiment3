#include <Wire.h>

//アドレス指定
#define GP2Y0E03_ADDR 0x40
#define DISTANCE_ADDR 0x5E

float distance = 0.0;
float previous_distance = 0.0;
float diff = 0.0;
int inhalation_flag;
float inhalation_threshold = 0.1;
unsigned long inhalation_start_time;
int exhalation_flag;
float exhalation_threshold = -0.1;
unsigned long exhalation_start_time;
float diff_array[5] = {0.0, 0.0, 0.0, 0.0, 0.0};
float low_pass_filtered_diff;
bool respiration_signal_detected = false;
bool inhaling = false;

void setup()
{
  Serial.begin(9600);//シリアル通信を9600bpsで初期化
  Wire.begin();//I2Cを初期化
  delay(50);//500msec待機(0.5秒待機)
}

void loop() {
  //変数宣言
  int dac[2];
  int i;

  Wire.beginTransmission(GP2Y0E03_ADDR);//I2Cスレーブ「Seeeduino XIAO」のデータ送信開始
  Wire.write(DISTANCE_ADDR);//距離の測定
  Wire.endTransmission();//I2Cスレーブ「Seeduino XIAO」のデータ送信終了

  Wire.requestFrom(GP2Y0E03_ADDR, 2);//I2Cデバイス「GP2Y0E03」に2Byteのデータ要求
  for (i=0; i<2; i++){
      dac[i] = Wire.read();//dacにI2Cデバイス「GP2Y0E03」のデータ読み込み
  }
  Wire.endTransmission();//I2Cスレーブ「Seeduino XIAO」のデータ送信終了

  distance = 10.0 * ((dac[0]*16.0+dac[1]) / 16.0) / (2.0*2.0); //距離(cm)を計算
  
  diff = distance - previous_distance; //前回のループの時との差分を計算
  previous_distance = distance;

  //移動平均計算用の配列を更新
  float diff_array_sum = 0.0;
  int j;
  int array_size = sizeof(diff_array) / sizeof(float);
  for (j = 0; j < array_size - 1; j = j + 1)
  {
    diff_array[j] = diff_array[j + 1];
    diff_array_sum += diff_array[j];
  }
  diff_array[array_size - 1] = diff;
  //移動平均(low_pass_filtered_diff)を計算
  diff_array_sum += diff;
  low_pass_filtered_diff = diff_array_sum / (float)(sizeof(diff_array) / sizeof(float));

  //閾値を更新
  update_threshold();

  //diff(前のループとの差分)が閾値を超えていたら呼気/吸気の開始と判定
  if(diff >= inhalation_threshold && !inhaling)
  {
    inhalation_flag = 1;
    inhaling = true;
    respiration_signal_detected = true;
    inhalation_start_time = millis();
  }
  else
  {
    inhalation_flag = 0;
  }

  if(diff <= exhalation_threshold && inhaling)
  {
    exhalation_flag = 1;
    inhaling = false;
    respiration_signal_detected = true;
    exhalation_start_time = millis();
  }
  else
  {
    exhalation_flag = 0;
  }

  // シリアル通信で情報を送信
  send_respiration_info();
  respiration_signal_detected = false;
  delay(50);
}

// 呼吸シグナルが検出されたか(bool), 吸気か(bool), 赤外線距離センサの値(float)をシリアル通信で送信
void send_respiration_info()
{
  byte send_buffer[6];
  send_buffer[0] = byte(respiration_signal_detected);
  send_buffer[1] = byte(inhaling);
  send_buffer[2] = ((uint8_t*)&distance)[0];
  send_buffer[3] = ((uint8_t*)&distance)[1];
  send_buffer[4] = ((uint8_t*)&distance)[2];
  send_buffer[5] = ((uint8_t*)&distance)[3];
  Serial.write( send_buffer, 6 );
}

// 呼気・吸気検出のための閾値を調整する関数
// ①閾値を超えて呼気/吸気が検出されてから100msは相次いで検知されることが無いように、絶対超えないだろう閾値に変更
// ①呼気(吸気)が検出されてから500msは吸気(呼気)が検出されないように、吸気(呼気)検出の閾値をっ絶対超えない値に変更
// ②その後は、時間がたつにつれて閾値が下がるように変更
void update_threshold()
{
  // 吸気開始判定の閾値の調整
  if (millis() - inhalation_start_time < 100 || millis() - exhalation_start_time < 500)
  {
    inhalation_threshold = 3.0;
  }
  else if (low_pass_filtered_diff > 0)
  {
    float multiplier = 100.0 * exp(- 1.0 * (millis() - inhalation_start_time) / 300.0);
    inhalation_threshold = min(multiplier * low_pass_filtered_diff + 0.10, 3.0);
  }
  else
  {
    inhalation_flag = 0;
    inhalation_threshold = 0.10;
  }

  // 呼気開始判定の閾値の調整
  if (millis() - inhalation_start_time < 500 || millis() - exhalation_start_time < 100)
  {
    exhalation_threshold = -3.0;
  }
  else if (low_pass_filtered_diff < 0)
  {
    float multiplier = 100.0 * exp(- 1.0 * (millis() - exhalation_start_time) / 300.0);
    exhalation_threshold = max(multiplier * low_pass_filtered_diff - 0.10, -3.0);
  }
  else
  {
    exhalation_flag = 0;
    exhalation_threshold = - 0.10;
  }
}
