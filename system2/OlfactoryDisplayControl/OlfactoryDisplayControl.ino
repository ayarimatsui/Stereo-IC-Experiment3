const int airPumpPin1 = 1;
const int airPumpPin2 = 2;
const int airPumpPin3 = 3;
const int airPumpPin4 = 6;
const int airPumpPin5 = 7;

int AirPumpPin = 0; // 0-5 0は停止
int SecondAirPumpPin = 0; // 0-5 0は停止
int Intensity = 0; // 0-255
bool PulseMode = false; // Pulse or Direct
int Duration = 0; // 0-8191 (millisec)
int Duty = 0; // 0-10

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(airPumpPin1, OUTPUT);
  pinMode(airPumpPin2, OUTPUT);
  pinMode(airPumpPin3, OUTPUT);
  pinMode(airPumpPin4, OUTPUT);
  pinMode(airPumpPin5, OUTPUT);
  Serial.begin(9600);
}

void loop() 
{
  char buf0, buf1, buf2, buf3, buf4;
   
  if(Serial.available()){
    buf0 = Serial.read();
    delay(1);
    if(buf0 == 61){
      digitalWrite(LED_BUILTIN, LOW);
      while(Serial.available()<=0);
      buf1 = Serial.read();
      while(Serial.available()<=0);
      buf2 = Serial.read();
      while(Serial.available()<=0);
      buf3 = Serial.read();
      while(Serial.available()<=0);
      buf4 = Serial.read();
      digitalWrite(LED_BUILTIN, HIGH);
  
      convertChtoBin(buf1,buf2, buf3, buf4);
      
      if (AirPumpPin == 0)
      {
        allPumpOff();
      }
      else
      {
        if (SecondAirPumpPin == 0)
        {
          if (PulseMode)
          {
            pulseInjection(AirPumpPin, Intensity, Duration, Duty);
          }
          else
          {
            directInjection(AirPumpPin, Intensity, Duration);
          }
        }
        else
        {
          doubleInjection(AirPumpPin, SecondAirPumpPin, Intensity, Duration);
        }
      }
    }
  }
  buf0 = 0;
}

void directInjection(int airPumpPin, int intensity, int duration)
{
  analogWrite(airPumpPin, intensity);
  delay(duration);
  analogWrite(airPumpPin, 0);
}

void pulseInjection(int airPumpPin, int intensity, int duration, int duty) //intensity:0-255, duration:0-65535, duty:0-10
{
  allPumpOff();
  
  unsigned long timerStart = millis();
  while (millis() - timerStart < duration)
  {
    analogWrite(airPumpPin, intensity);
    delay(duty);
    analogWrite(airPumpPin, 0);
    delay(10 - duty);
  }
  analogWrite(airPumpPin, 0);
}

void doubleInjection(int firstAirPumpPin, int secondAirPumpPin, int intensity, int duration) //intensity:0-255, duration:0-65535, duty:0-10
{
  allPumpOff();
  
  unsigned long timerStart = millis();
  while (millis() - timerStart < duration)
  {
    analogWrite(firstAirPumpPin, intensity);
    delay(20);
    analogWrite(firstAirPumpPin, 0);
    delay(5);
    analogWrite(secondAirPumpPin, intensity);
    delay(20);
    analogWrite(secondAirPumpPin, 0);
    delay(5);
  }
  analogWrite(firstAirPumpPin, 0);
  analogWrite(secondAirPumpPin, 0);
}

void allPumpOff()
{
  analogWrite(airPumpPin1, 0);
  analogWrite(airPumpPin2, 0);
  analogWrite(airPumpPin3, 0);
  analogWrite(airPumpPin4, 0);
  analogWrite(airPumpPin5, 0);
}

void convertChtoBin(char buf1, char buf2, char buf3, char buf4)
{
  int airPumpID = 0;
  if((buf1 & 0x80) != 0) airPumpID = airPumpID + 4;
  if((buf1 & 0x40) != 0) airPumpID = airPumpID + 2;
  if((buf1 & 0x20) != 0) airPumpID = airPumpID + 1;

  switch (airPumpID)
  {
    case 0:
      AirPumpPin = 0;
      break;
    case 1:
      AirPumpPin = airPumpPin1;
      break;
    case 2:
      AirPumpPin = airPumpPin2;
      break;
    case 3:
      AirPumpPin = airPumpPin3;
      break;
    case 4:
      AirPumpPin = airPumpPin4;
      break;
    case 5:
      AirPumpPin = airPumpPin5;
      break;
  }

  int intensity = 0;
  if((buf1 & 0x10) != 0) intensity = intensity + 128;
  if((buf1 & 0x08) != 0) intensity = intensity + 64;
  if((buf1 & 0x04) != 0) intensity = intensity + 32;
  if((buf1 & 0x02) != 0) intensity = intensity + 16;
  if((buf1 & 0x01) != 0) intensity = intensity + 8;
  if((buf2 & 0x80) != 0) intensity = intensity + 4;
  if((buf2 & 0x40) != 0) intensity = intensity + 2;
  if((buf2 & 0x20) != 0) intensity = intensity + 1;
  Intensity = intensity;

  int secondAirPumpPinID = 0;
  if((buf2 & 0x10) != 0) secondAirPumpPinID = secondAirPumpPinID + 4;
  if((buf2 & 0x08) != 0) secondAirPumpPinID = secondAirPumpPinID + 2;
  if((buf2 & 0x04) != 0) secondAirPumpPinID = secondAirPumpPinID + 1;

  switch (secondAirPumpPinID)
  {
    case 0:
      SecondAirPumpPin = 0;
      break;
    case 1:
      SecondAirPumpPin = airPumpPin1;
      break;
    case 2:
      SecondAirPumpPin = airPumpPin2;
      break;
    case 3:
      SecondAirPumpPin = airPumpPin3;
      break;
    case 4:
      SecondAirPumpPin = airPumpPin4;
      break;
    case 5:
      SecondAirPumpPin = airPumpPin5;
      break;
  }

  int mode = 0;
  if((buf2 & 0x02) != 0) mode = mode + 1;
  if (mode == 1)
  {
    PulseMode = true;
  }
  else
  {
    PulseMode = false;
  }

  int duration = 0;
  if((buf2 & 0x01) != 0) duration = duration + 4096;
  if((buf3 & 0x80) != 0) duration = duration + 2048;
  if((buf3 & 0x40) != 0) duration = duration + 1024;
  if((buf3 & 0x20) != 0) duration = duration + 512;
  if((buf3 & 0x10) != 0) duration = duration + 256;
  if((buf3 & 0x08) != 0) duration = duration + 128;
  if((buf3 & 0x04) != 0) duration = duration + 64;
  if((buf3 & 0x02) != 0) duration = duration + 32;
  if((buf3 & 0x01) != 0) duration = duration + 16;
  if((buf4 & 0x80) != 0) duration = duration + 8;
  if((buf4 & 0x40) != 0) duration = duration + 4;
  if((buf4 & 0x20) != 0) duration = duration + 2;
  if((buf4 & 0x10) != 0) duration = duration + 1;
  Duration = duration;

  int duty = 0;
  if((buf4 & 0x08) != 0) duty = duty + 8;
  if((buf4 & 0x04) != 0) duty = duty + 4;
  if((buf4 & 0x02) != 0) duty = duty + 2;
  if((buf4 & 0x01) != 0) duty = duty + 1;
  Duty = duty;
}
