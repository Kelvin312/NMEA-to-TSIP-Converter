/*
 * NmeaParser.cpp
 *
 * Created: 02.07.2017 20:51:05
 *  Author: Kelvin
 */ 
#include "stdafx.h"

 class NmeaParser
 {
 #define MSG_ENCODE(_a,_b) ((_a)|(u16(_b)<<8))
 #define DDF_MULTI(_x) (M_PI /180 /60 /DecimalDivisor[(_x)])
 
const bool Error = true;
const bool Ok = false;
const u16 DecimalDivisor[5] = {1,10,100,1000,10000};
const u8 MaxDecimalDivisor = 4;
bool globalError;
u8 byteCount;
u8 comaPoint;
u8 charPoint;
u8 checkSum;
u8 data;

u8 Hex2Int(u8 c)
{
	if (c >= '0' && c <= '9')
	return c - '0';
	if (c >= 'A' && c <= 'F')
	return c - 'A' + 10;
	if (c >= 'a' && c <= 'f')
	return c - 'a' + 10;
	globalError = Error;
	return 0;
}

struct 
{
	u32 seconds;
	u8 fractionSeconds;
	u8 day;
	u8 month;
	u8 year;
	float gpsTimeOfWeek;
	u16 gpsWeekNumber;
	float gpsUtcOffset;
} nmeaDateTime;

struct
{
	s32 latitudeMinutes;
	u8 latitudeDivisor;
	s32 longitudeMinutes;
	u8 longitudeDivisor;
	float latitudeRadians;
	float longitudeRadians;
	float altitudeMeters;
} nmeaPosition;

void PositionCalc()
{
	nmeaPosition.latitudeRadians = nmeaPosition.latitudeMinutes * DDF_MULTI(nmeaPosition.latitudeDivisor);
	nmeaPosition.longitudeRadians = nmeaPosition.longitudeMinutes * DDF_MULTI(nmeaPosition.longitudeDivisor);
}

void GetTime()
{
	switch(charPoint)
	{
		case 0:
		nmeaDateTime.fractionSeconds = 0;
		nmeaDateTime.seconds = Hex2Int(data);
		break;
		case 1:
		case 3:
		case 5:
		nmeaDateTime.seconds *= 10;
		nmeaDateTime.seconds += Hex2Int(data);
		break;
		case 2:
		case 4:
		nmeaDateTime.seconds *= 6;
		nmeaDateTime.seconds += Hex2Int(data);
		break;
		case 7:
		nmeaDateTime.fractionSeconds = Hex2Int(data)*10;
		break;
		case 8:
		nmeaDateTime.fractionSeconds += Hex2Int(data);
		break;
	}
}

void GetDate()
{
	switch(charPoint)
	{
		case 0:
		nmeaDateTime.month = 0;
		nmeaDateTime.year = 0;
		nmeaDateTime.day = Hex2Int(data);
		break;
		case 1:
		nmeaDateTime.day *= 10;
		nmeaDateTime.day += Hex2Int(data);
		break;
		case 2:
		nmeaDateTime.month = Hex2Int(data);
		break;
		case 3:
		nmeaDateTime.month *= 10;
		nmeaDateTime.month += Hex2Int(data);
		break;
		case 4:
		nmeaDateTime.year = Hex2Int(data);
		break;
		case 5:
		nmeaDateTime.year *= 10;
		nmeaDateTime.year += Hex2Int(data);
		
		u16 m = nmeaDateTime.month;
		u16 y = nmeaDateTime.year;
		if (m > 2) { m -= 3; }
		else	   { m += 12 - 3; --y;}
		if(y >= 80)
		{
			globalError = Error;
			break;
		}
		//https://ru.stackoverflow.com/questions/455831/%D0%92%D1%8B%D1%87%D0%B8%D1%81%D0%BB%D0%B8%D1%82%D1%8C-%D0%BA%D0%BE%D0%BB%D0%B8%D1%87%D0%B5%D1%81%D1%82%D0%B2%D0%BE-%D0%B4%D0%BD%D0%B5%D0%B9-%D0%B1%D0%B5%D0%B7-%D0%BC%D0%B0%D1%81%D1%81%D0%B8%D0%B2%D0%B0-%D0%B8-%D1%80%D0%B5%D0%BA%D1%83%D1%80%D1%81%D0%B8%D0%B8
		//Количество дней с January 6, 1980
		u16 gpsTime = nmeaDateTime.day + (153 * m + 2) / 5 + 365 * y + (y >> 2) + 7359;//- y / 100 + y / 400 - 723126;
		nmeaDateTime.gpsWeekNumber = gpsTime / 7;
		nmeaDateTime.gpsTimeOfWeek = u32(gpsTime % 7)*24*60*60 + nmeaDateTime.seconds + float(nmeaDateTime.fractionSeconds)/100;
		nmeaDateTime.gpsUtcOffset = 0;
		break;
	}
}

void GetLatitude()
{
	switch(charPoint)
	{
	case 0:
	nmeaPosition.latitudeDivisor = 0;
	nmeaPosition.latitudeMinutes = Hex2Int(data);
	break;
	case 1:
	case 3:
	nmeaPosition.latitudeMinutes *= 10;
	nmeaPosition.latitudeMinutes += Hex2Int(data);
	break;
	case 2:
	nmeaPosition.latitudeMinutes *= 6;
	nmeaPosition.latitudeMinutes += Hex2Int(data);
	break;
	case 4: break;
	default:
	if(nmeaPosition.latitudeDivisor < MaxDecimalDivisor)
	{
		nmeaPosition.latitudeMinutes *= 10;
		nmeaPosition.latitudeMinutes += Hex2Int(data);
		++nmeaPosition.latitudeDivisor;
	}
	break;
	}
}

void GetLongitude()
{
	switch(charPoint)
	{
	case 0:
	nmeaPosition.longitudeDivisor = 0;
	nmeaPosition.longitudeMinutes = Hex2Int(data);
	break;
	case 1:
	case 2:
	case 4:
	nmeaPosition.longitudeMinutes *= 10;
	nmeaPosition.longitudeMinutes += Hex2Int(data);
	break;
	case 3:
	nmeaPosition.longitudeMinutes *= 6;
	nmeaPosition.longitudeMinutes += Hex2Int(data);
	break;
	case 5: break;
	default:
	if(nmeaPosition.longitudeDivisor < MaxDecimalDivisor)
	{
		nmeaPosition.longitudeMinutes *= 10;
		nmeaPosition.longitudeMinutes += Hex2Int(data);
		++nmeaPosition.longitudeDivisor;
	}
	break;
	}
}

void GetFloat(float &param)
{
	static bool isSign;
	static bool isDot;
	static u8 divisor;
	if(charPoint == 0)
	{
		isSign = false;
		isDot = false;
		param = 0;
		divisor = 0;
	}
	switch(data)
	{
		case ' ':
		case '+': break;
		case '-': isSign = true; break;
		case '.': isDot = true; break;
		default:
		if(isDot)
		{
			if(divisor < MaxDecimalDivisor)
			{
				++divisor;
				float temp = Hex2Int(data);
				temp /= DecimalDivisor[divisor];
				if(isSign)
				{
					param -= temp;
				}
				else
				{
					param += temp;
				}
			}
		}
		else
		{
			param *= 10;
			if(isSign)
			{
				param -= Hex2Int(data);
			}
			else
			{
				param += Hex2Int(data);
			}
		}
		break;
	}
}

 public:
 
 
 
bool Parse(u8 c)
{
	data = c;
	static union
	{
		u16 msg16;
		u8 msg8[2];
	} msgType;
	
	globalError = Ok;
	checkSum ^= data;
	
	switch(++byteCount)
	{
	case 1: if(data!='$') byteCount=0; checkSum = 0; break;
	case 2: if(data!='G') byteCount=0; break;
	case 3: if(data!='P' && data!='N') byteCount=0; break;
	case 4: if(data!='R' && data!='G') byteCount=0; break;
	case 5: msgType.msg8[0] = data; break;
	case 6: msgType.msg8[1] = data; 
	comaPoint = 0;
	charPoint = 0;
	break;
	case 7: 
	if(data != '*')
	{ 
		byteCount = 6;
		if(data == ',')
		{
			++comaPoint;
			charPoint = 0;
			break;
		}
		switch(msgType.msg16)
		{
			case MSG_ENCODE('M','C'): //RMC - Recommended Minimum Specific
			switch(comaPoint)
			{
				case 1: GetTime(); break; //UTC of Position Fix 
				case 2: break; //Status: A = Valid, V = navigation receiver warning
				case 3: GetLatitude(); break; //Latitude
				case 4: if(data == 'S') nmeaPosition.latitudeMinutes *= -1; break; //Latitude, N (North) or S (South)
				case 5: GetLongitude(); break; //Longitude
				case 6: if(data == 'W') nmeaPosition.longitudeMinutes *= -1; break; //E (East) or W (West).
				case 7: break; //Speed over the ground (SOG) in knots
				case 8: break; //Track made good in degrees true
				case 9: GetDate(); break; //Date: dd/mm/yy
				case 10: break; //Magnetic variation in degrees
				case 11: break;//E = East / W= West
				case 12: break;//Position System Mode Indicator;
			}
			
			break;
			case MSG_ENCODE('G','A'):
			break;
			case MSG_ENCODE('S','A'):
			break;
			case MSG_ENCODE('S','V'):
			break;
		}
		++charPoint;
	}
	break;
	case 8:
	checkSum ^= Hex2Int(data) << 4;
	break;
	case 9:
	checkSum ^= Hex2Int(data) ^ '*';
	break;
	
	}
	if(globalError == Error) byteCount = 0;
	return globalError;
	}
	
	
 };