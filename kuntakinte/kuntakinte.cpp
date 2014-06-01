// kuntakinte.cpp
#include "pch.h"
#include "kuntakinte.h"

using namespace kuntakinte;
using namespace Platform;

//namespace required to support sensor and events
using namespace Windows::Devices::Sensors;
using namespace Windows::Foundation;
using namespace Windows::System::Threading;

using namespace Windows::Foundation::Collections;

//namespace for UI control
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Media;


#define ROLL	0
#define PITCH	1
#define YAW		2




flightbox::flightbox()
{

	//get inclinometer
	inclinometer = Inclinometer::GetDefault();	
	inclinometer->ReportInterval = inclinometer->MinimumReportInterval;	
	inclinometer->ReadingChanged::add(ref new TypedEventHandler<Inclinometer ^, InclinometerReadingChangedEventArgs ^>(this, &flightbox::OnInclineReadingChanged));

	//get gyroscope
	gyrometer = Gyrometer::GetDefault();
	gyrometer->ReportInterval = gyrometer->MinimumReportInterval;
	//gyrometer->ReportInterval = 4;
	
	gyrometer->ReadingChanged::add(ref new TypedEventHandler<Gyrometer^, GyrometerReadingChangedEventArgs^>(this, &flightbox::OnGyroReadingChanged));	

	//get accelerometer
	accelerometer = Accelerometer::GetDefault();
	accelerometer->ReportInterval = accelerometer->MinimumReportInterval;
	accelerometer->ReadingChanged::add(ref new TypedEventHandler<Accelerometer ^, AccelerometerReadingChangedEventArgs ^>(this, &flightbox::OnAccelReadingChanged));
	
	tickgyro = gyrometer->ReportInterval / 1000.0;
	tickincline = inclinometer->ReportInterval / 1000.0;
	tickaccel = accelerometer->ReportInterval / 1000.0;
	//set PID gain of roll loop
	
	rollGain[0] = 2;
	rollGain[1] = 0.01;
	rollGain[2] = 0.2; 

	rollRateG[0] = 0.3;
	rollRateG[1] = 0.001;
	rollRateG[2] = 0.2;

	//set PID gain of pitch loop
	pitchGain[0] = 2;
	pitchGain[1] = 0.01;
	pitchGain[2] = 0.2;

	pitchRateG[0] = 0.3;
	pitchRateG[1] = 0.001;
	pitchRateG[2] = 0.2;

	//set PID gain of yaw loop
	yawGain[0] = 1;
	yawGain[1] = 1;
	yawGain[2] = 1;
	

	//init omega array
	omega = ref new Platform::Array<float>(3);
	//initialize rpy array
	rpy = ref new Platform::Array<float>(3);
	//init position array
	position = ref new Platform::Array<float>(3);
	//init motors array
	motors = ref new Platform::Array<int>(4);

	offset = 1000;
	cmdRollRate = 0;
	cmdPitchRate = 0;
	
	rollRateE = 0;
	rollRateEint = 0;
	
	pitchRateE = 0;
	pitchRateEint = 0;

	
}


void flightbox::initBt(){

	/*
	PeerFinder::FindAllPeersAsync();
	

	socket = ref new StreamSocket();
	socket->ConnectAsync(;
	dataWriter = ref new DataWriter(socket->OutputStream);
	*/
}

int flightbox::calibrate(float roll, float pitch, float yaw){

	//set calibration value if there is offset
	this->mroll = roll;
	this->mpitch = pitch;
	this->myaw = yaw;

	return 0;

	
}

int flightbox::rollPID(float rP,float rI,float rD){
	rollGain[0] = rP;
	rollGain[1] = rI;
	rollGain[2] = rD;
	return 0;
}

int flightbox::pitchPID(float pP,float pI,float pD){
	pitchGain[0] = pP;
	pitchGain[1] = pI;
	pitchGain[2] = pD;
	return 0;
}

int flightbox::yawPID(float yP,float yI,float yD){
	return 0;
}


void flightbox::OnInclineReadingChanged(Inclinometer ^sender, InclinometerReadingChangedEventArgs ^args)
{


	//roll and pitch goes from -180 to 180
	//integrate roll; running sum
	
	//integrate yaw
	//yaw goes from 0 to 360 NEED TO ACCOUNT FOR THIS //////////////////////
	
	
	float rollEprev = 0-rpy[ROLL];
	float pitchEprev = 0-rpy[PITCH];

	rpy[ROLL] = 0-args->Reading->RollDegrees;
	rpy[PITCH] = 0-args->Reading->PitchDegrees;
	rpy[YAW] = 0-args->Reading->YawDegrees;

	rollEint += (rollEprev + rpy[ROLL]) / 2 * tickincline;
	pitchEint += (pitchEprev + rpy[PITCH]) / 2 * tickincline;


	cmdRollRate = rollGain[0] * rpy[ROLL] + rollGain[1] * rollEint + rollGain[2] * (-omega[ROLL]);
	cmdPitchRate = pitchGain[0] * rpy[PITCH] + pitchGain[1] * pitchEint + pitchGain[2] * (-omega[PITCH]);

	
	//inclineEvent(rpy);
}

void flightbox::OnGyroReadingChanged(Gyrometer^sender, GyrometerReadingChangedEventArgs ^args){
	


	omega[ROLL] = args->Reading->AngularVelocityY;
	omega[PITCH] = args->Reading->AngularVelocityX;
	omega[YAW] = args->Reading->AngularVelocityZ;

	float rollRateEprev = rollRateE;
	float pitchRateEprev = pitchRateE;

	rollRateE = cmdRollRate - omega[ROLL];
	pitchRateE = cmdPitchRate - omega[PITCH];
	
	rollRateEint += (rollRateEprev + rollRateE) / 2 * tickgyro;
	pitchRateEint += (pitchRateEprev + pitchRateE) / 2 * tickgyro;

	//calculate PI for roll
	motors[0] = rollRateG[0] * rollRateE+rollRateG[1]*rollRateEint;
	motors[2] = offset - motors[0];
	motors[0] = offset + motors[0];



	//calculate PI for pitch
	motors[1] = pitchRateG[0] * pitchRateE+pitchRateG[1]*pitchRateEint;
	motors[3] = offset - motors[1];
	motors[1] = offset + motors[1];


	//inclineEvent(omega);
	motorEvent(motors);
	
}


void flightbox::OnAccelReadingChanged(Accelerometer ^sender, AccelerometerReadingChangedEventArgs ^args)
{

}

void flightbox::throttle(float incr){
	offset = 1000 + incr;
}