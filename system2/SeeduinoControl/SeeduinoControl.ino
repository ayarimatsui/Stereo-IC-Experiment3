#define PI 3.141592653589793

//wave func
float bipole(float phase, float duty=0.5){
  if(phase < duty){
    return 1;
  }else{
    return -1;
  }
}
float posimono(float phase, float duty=0.5){
  if(phase < 0.5){
    return 1;
  }else{
    return 0;
  }
}
float negamono(float phase, float duty=0.5){
  if(phase < duty){
    return -1;
  }else{
    return 0;
  }
}
float posicon(float phase, float duty=0.5){
  return 1;
}
float negacon(float phase, float duty=0.5){
  return -1;
}
float sine(float phase, float duty=0.5){
  return sin(2 * PI * phase);
}
float positrap(float phase, float duty=0.5)
{
  if(phase < 0.25){
    return phase / 0.25;
  }else if(phase >= 0.25 && phase <= 0.5){
    return 1;
  }else if(phase > 0.5 && phase < 0.75){
    return 1 - (phase - 0.5) / 0.25;
  }else{
    return 0;
  }
}
float negatrap(float phase, float duty=0.5)
{
  if(phase < 0.25){
    return -1 * phase / 0.25;
  }else if(phase >= 0.25 && phase <= 0.5){
    return -1;
  }else if(phase > 0.5 && phase < 0.75){
    return -1 + (phase - 0.5) / 0.25;
  }else{
    return 0;
  }
}
//transition func
float constant(float phase){
  return 1;
}
float linear(float phase){
  return phase;
}
float smooth(float phase){
  return phase * phase * (3 - 2 * phase);
}
float lineardecay(float phase){
  return 1 - phase;
}
float smoothdecay(float phase)
{
  return (cos(PI * phase) + 1) / 2.0;
}
float (*waves[])(float, float) = {bipole, posimono, negamono, posicon, negacon, sine, positrap, negatrap};
float (*transitions[])(float) = {constant, linear, smooth, lineardecay, smoothdecay, smooth, smooth, smooth};

//const int analogOutPin = A0; // Analog output pin that the LED is attached to
int g_Ch = 0;
int g_Val = 0;//0-255
int g_Freq = 0;//0-1023
float g_Transition_Speed = 0;
float (*g_Wave)(float, float)= bipole;
float g_Duty = 0.5;
float (*g_Transition)(float) = constant;
int data[3] = {0,0,0};
char key = 0;
int g_polarity = 0;

float t = 0;
float ts = 0;
float te = 0;
//for transition
float t_trans = 0;

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  pinMode(1, OUTPUT);
  pinMode(2, OUTPUT);
  
  Serial1.begin(9600);
}

int setPolarity(int value){
  if(value > 0){
    digitalWrite(1, HIGH);
    digitalWrite(2, LOW);
    return value;
  }else{
    digitalWrite(2, HIGH);
    digitalWrite(1, LOW);
    return -value;
  }
}

void loop()
{
  char Dat0, Dat1, Dat2, Dat3, Dat4, Dat5;
   
  if(Serial1.available()){
    Dat0 = Serial1.read();
    delay(1);
    if(Dat0 == 71){
      digitalWrite(LED_BUILTIN, LOW);
      while(Serial1.available()<=0);
      Dat1 = Serial1.read();
      while(Serial1.available()<=0);
      Dat2 = Serial1.read();
      while(Serial1.available()<=0);
      Dat3 = Serial1.read();
      while(Serial1.available()<=0);
      Dat4 = Serial1.read();
      while(Serial1.available()<=0);
      Dat5 = Serial1.read();
      digitalWrite(LED_BUILTIN, HIGH);
  
      ConvertChtoBin(Dat1,Dat2, Dat3, Dat4, Dat5);
    }
    if(Dat0 == 67){
      delay(g_Ch);
      Serial1.print(g_Ch);
    }
  }
    
    
  //update
  te = micros()/(float)1000000;
  float diff = te - ts;
  
  ts = te;
  t = t + diff * g_Freq;
  t = t - (int)t;
  
  t_trans = t_trans + diff * g_Freq * g_Transition_Speed;
  t_trans = min(t_trans, 1);
  
  int value = g_Val * g_Wave(t, g_Duty) * g_Transition(t_trans);
  value = setPolarity(value);
  analogWrite(A0, (char)(value));
  Dat0 = 0;
}

void ConvertChtoBin(char dat1, char dat2, char dat3, char dat4, char dat5)
{
    int ch =0;
    
    if((dat1 & 0x80) != 0) ch = ch + 4;
    if((dat1 & 0x40) != 0) ch = ch + 2;
    if((dat1 & 0x20) != 0) ch= ch + 1;

    if(g_Ch == ch){
        
        int current = 0;
        int frequency = 0;
        int wave = 0;
        int duty = 0;
        int transition_Duration = 0;
        int transition = 0;
        
        if((dat1 & 0x10) != 0) current = current + 2048;
        if((dat1 & 0x08) != 0) current = current + 1024;
        if((dat1 & 0x04) != 0) current = current + 512;
        if((dat1 & 0x02) != 0) current = current + 256;
        if((dat1 & 0x01) != 0) current = current + 128;
        if((dat2 & 0x80) != 0) current = current + 64;
        if((dat2 & 0x40) != 0) current = current + 32;
        if((dat2 & 0x20) != 0) current = current + 16;
        if((dat2 & 0x10) != 0) current = current + 8;
        if((dat2 & 0x08) != 0) current = current + 4;
        if((dat2 & 0x04) != 0) current = current + 2;
        if((dat2 & 0x02) != 0) current = current + 1;

        g_Val = (int) (current / 16);

        if((dat2 & 0x01) != 0) frequency = frequency + 512;
        if((dat3 & 0x80) != 0) frequency = frequency + 256;
        if((dat3 & 0x40) != 0) frequency = frequency + 128;
        if((dat3 & 0x20) != 0) frequency = frequency + 64;
        if((dat3 & 0x10) != 0) frequency = frequency + 32;
        if((dat3 & 0x08) != 0) frequency = frequency + 16;
        if((dat3 & 0x04) != 0) frequency = frequency + 8;
        if((dat3 & 0x02) != 0) frequency = frequency + 4;
        if((dat3 & 0x01) != 0) frequency = frequency + 2;
        if((dat4 & 0x80) != 0) frequency = frequency + 1;

        g_Freq = frequency;
        
        if((dat4 & 0x40) != 0) wave = wave + 4;
        if((dat4 & 0x20) != 0) wave = wave + 2;
        if((dat4 & 0x10) != 0) wave = wave + 1;

        g_Wave = waves[wave];

        if((dat4 & 0x08) != 0) duty = duty + 8;
        if((dat4 & 0x04) != 0) duty = duty + 4;
        if((dat4 & 0x02) != 0) duty = duty + 2;
        if((dat4 & 0x01) != 0) duty = duty + 1;

        if(duty < 10){
          g_Duty = (float)duty * 0.1;
        }
        else{
          g_Duty = 1.0;
        }

        if((dat5 & 0x80) != 0) transition_Duration = transition_Duration + 8;
        if((dat5 & 0x40) != 0) transition_Duration = transition_Duration + 4;
        if((dat5 & 0x20) != 0) transition_Duration = transition_Duration + 2;
        if((dat5 & 0x10) != 0) transition_Duration = transition_Duration + 1;

        if(transition_Duration == 0){
          g_Transition_Speed = 0;
        }else{
          g_Transition_Speed = 1.0 / (float)transition_Duration;
        }
        if((dat5 & 0x08) != 0) transition = transition + 4;
        if((dat5 & 0x04) != 0) transition = transition + 2;
        if((dat5 & 0x02) != 0) transition = transition + 1;

        g_Transition = transitions[transition];
        //reset transition time
        t_trans = 0;
    }
}
