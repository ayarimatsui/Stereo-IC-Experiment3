const int SW1 = 5;
const int SW2 = 6;
const int SW3 = 7;
const int SW4 = 8;

void turnOnLeftGNS()
{
  Serial.println("LeftGNS");
  analogWrite(SW1, 255);
  analogWrite(SW2, 0);
}

void turnOnLeftSham()
{
  Serial.println("LeftSham");
  analogWrite(SW1, 0);
  analogWrite(SW2, 255);
}

void turnOnRightGNS()
{
  Serial.println("RightGNS");
  analogWrite(SW3, 255);
  analogWrite(SW4, 0);
}

void turnOnRightSham()
{
  Serial.println("RightSham");
  analogWrite(SW3, 0);
  analogWrite(SW4, 255);
}

void turnOff()
{
  Serial.println("off");
  analogWrite(SW1, 0);
  analogWrite(SW2, 0);
  analogWrite(SW3, 0);
  analogWrite(SW4, 0);
}

String serialRead()
{
  while (Serial.available() == 0) {}     //wait for data available
  String data = Serial.readString();  //read until timeout
  data.trim();
  return data;
}

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(SW1, OUTPUT);
  pinMode(SW2, OUTPUT);
  pinMode(SW3, OUTPUT);
  pinMode(SW4, OUTPUT);

  Serial.setTimeout(10);
  Serial1.begin(9600);
}

void loop()
{
  String cmd = serialRead();
  if (cmd == "LeftGNS")
  {
    turnOnLeftGNS();
  }
  else if (cmd == "LeftSham")
  {
    turnOnLeftSham();
  }
  else if (cmd == "RightGNS")
  {
    turnOnRightGNS();
  }
  else if (cmd == "RightSham")
  {
    turnOnRightSham();
  }
  else
  {
    turnOff();
  }
}
