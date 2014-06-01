#include <Servo.h>
#include <SoftwareSerial.h>

#define M9  9
#define M6  6
#define M5  5
#define M3  3

Servo servo9;
Servo servo6;
Servo servo5;
Servo servo3;

/********************
brushless esc wire 
orange = signal
red = plus
brown is ground
********************/

int i=0;
float cmdOut = 0;
 
int m9pwm=0;
int m6pwm=0;
int m5pwm=0;
int m3pwm=0;

int cmd=0; 
String echo="";



void setup() {
  
  //initialize serial comm through bluetooth 1 0
  Serial.begin(38400); //38400
  Serial.println("USB Connected");

  
  //servo init
  //attach dedicate pin to servo
  servo9.attach(M9,1000,2000);
  servo6.attach(M6,1000,2000);
  servo5.attach(M5,1000,2000);
  servo3.attach(M3,1000,2000);
  ///////////////////////////////

  
}


void loop() {
  
  //read bluetooth serial buffer
  
  while(Serial.available()>=8){
    /*
    //m9pwm=0;
    m9pwm=(Serial.read()<<8)|Serial.read();
    
    //m5pwm=0;

    m5pwm=(Serial.read()<<8)|Serial.read();
    //m3pwm=0;

    m3pwm=(Serial.read()<<8)|Serial.read();
    //m6pwm=0;


    m6pwm = (Serial.read()<<8)|Serial.read();
    */
    servo3.writeMicroseconds((Serial.read()<<8)|Serial.read()); 
    servo6.writeMicroseconds((Serial.read()<<8)|Serial.read());
    servo9.writeMicroseconds((Serial.read()<<8)|Serial.read());    
    servo5.writeMicroseconds((Serial.read()<<8)|Serial.read());
  
  
  }

}
