/*
* NmeaParser.cpp
*
* Created: 02.07.2017 20:51:05
*  Author: Kelvin
*/

#ifndef NMEA_PARSER_H_
#define NMEA_PARSER_H_

#include "stdafx.h"
#include "HardwareUART.cpp"

class NmeaParser
{
	public:
	
	volatile u16 ppsTimeMSec;
	volatile u8 ppsTimeSec;
	volatile bool isHealthSend;
	
	
	HardUart &tsipUart;
	//void (*tsipPushRaw)(u8);
	NmeaParser(HardUart tsipUart):tsipUart(tsipUart)
	{
		nmeaHealth.HealthClear();
	}
	
	inline void TsipPushRaw(u8 c)
	{
		tsipUart.TransmitAndWait(c);
	}
	inline void DebugPush(u8 c)
	{
		//tsipUart.TransmitAndWait(c);
	}
	
	private:
	#define MSG_ENCODE(_a,_b) ((_a)|(u16(_b)<<8))
	
	
	const u8 DLE = 0x10;
	const u8 ETX = 0x03;
	float Zero = 0;
	const bool Error = true;
	const bool Ok = false;
	const u16 DecimalDivisor[5] = {1,10,100,1000,10000};
	const u8 MaxDecimalDivisor = 4;

	bool globalError; //Флаг ошибки пакета NMEA
	u8 byteCount; //Индекс типа данных
	u8 comaPoint; //Номер поля данных
	u8 charPoint; //Номер символа в текущем поле данных
	u8 checkSum; //Контрольная сумма
	u8 data; //Обрабатываемый байт NMEA

	u8 Hex2Int() //Перевод ASCII в число
	{
		if (data >= '0' && data <= '9')
		return data - '0';
		if (data >= 'A' && data <= 'F')
		return data - 'A' + 10;
		if (data >= 'a' && data <= 'f')
		return data - 'a' + 10;
		globalError = Error;
		return 0;
	}

	u16 updateFlag; //Флаги обновления данных

	enum
	{
		UpdateTime = 1,
		UpdateDate = 2,
		UpdateLatitude = 4,
		UpdateLongitude = 8,
		UpdateAltitude = 16,
		UpdateSpeed = 32,
		UpdateCourse = 64,
		UpdateDateTime = UpdateTime | UpdateDate,
		UpdatePosition = UpdateLatitude | UpdateLongitude | UpdateAltitude,
		UpdateVelocity = UpdateSpeed | UpdateCourse,
		UpdateDimension = 128,
		UpdatePrn  = 256,
		UpdatePDop = 512,
		UpdateHDop = 1024,
		UpdateVDop = 2048,
		UpdatePrecision = UpdateDimension | UpdatePDop | UpdateHDop | UpdateVDop,
		UpdateQuality = 4096
	};

	struct
	{
		s32 seconds; //Секунд в текущем дне
		u8 centiSeconds; //Сотых секунд
		u8 day;
		u8 month;
		u8 year;
		float gpsTimeOfWeek; //Секунд в текущей GPS неделе
		u16 gpsWeekNumber; //Расширенный номер текущей GPS недели
		float gpsUtcOffset; //Смещение времени GPS UTC секунд
		const u8 gpsUtcOffsetConst = 18; //то-же, но задано константой
		u32 gpsTimeFixSec; //День недели и UTC смещение в секундах
		float timeOfFix; //Время, когда был сделан замер координат/скорости.
		const u32 secInDay = u32(24)*60*60; //Секунд в сутках
		
		void TimeOfFixCalc()
		{
			timeOfFix = gpsTimeFixSec + seconds + float(centiSeconds)/100;
			if(timeOfFix >= 7 * secInDay)
			{
				timeOfFix -= 7 * secInDay;
			}
		}

		bool DateTimeCalc() //Вычисление времени недели и номера недели GPS
		{
			u16 m = month;
			u16 y = year;
			if (m > 2) { m -= 3; }
			else	   { m += 12 - 3; --y;}
			if(y >= 80)
			{
				return true;
			}
			//https://ru.stackoverflow.com/questions/455831/%D0%92%D1%8B%D1%87%D0%B8%D1%81%D0%BB%D0%B8%D1%82%D1%8C-%D0%BA%D0%BE%D0%BB%D0%B8%D1%87%D0%B5%D1%81%D1%82%D0%B2%D0%BE-%D0%B4%D0%BD%D0%B5%D0%B9-%D0%B1%D0%B5%D0%B7-%D0%BC%D0%B0%D1%81%D1%81%D0%B8%D0%B2%D0%B0-%D0%B8-%D1%80%D0%B5%D0%BA%D1%83%D1%80%D1%81%D0%B8%D0%B8
			//Количество дней с January 6, 1980
			u16 gpsTime = day + (153 * m + 2) / 5 + 365 * y + (y >> 2) + 7359;//- y / 100 + y / 400 - 723126;
			gpsWeekNumber = gpsTime / 7;
			gpsTimeFixSec = (gpsTime % 7) * secInDay + gpsUtcOffsetConst;
			gpsTimeOfWeek = gpsTimeFixSec + seconds + float(centiSeconds)/100;
			gpsUtcOffset = gpsUtcOffsetConst;
			if(gpsTimeOfWeek >= 7 * secInDay)
			{
				gpsTimeOfWeek -= 7 * secInDay;
				gpsWeekNumber += 1;
			}
			return false;
		}
	} nmeaDateTime;

	struct
	{
		s32 latitudeMinutes;
		u8 latitudeDivisor;
		s32 longitudeMinutes;
		u8 longitudeDivisor;
		float latitudeRadians;
		float longitudeRadians;
		float mslAltitudeMeters;
		float haeAltitudeMeters;
		float mslAboveHae;
		
		#define DDF_MULTI(_x) (M_PI /180 /60 /(_x))
		const float DecimalDivisorF[5] = {DDF_MULTI(1),DDF_MULTI(10),DDF_MULTI(100),DDF_MULTI(1000),DDF_MULTI(10000)};
		void PositionCalc()
		{
			latitudeRadians = latitudeMinutes * DecimalDivisorF[latitudeDivisor];
			longitudeRadians = longitudeMinutes * DecimalDivisorF[longitudeDivisor];
			haeAltitudeMeters = mslAltitudeMeters + mslAboveHae;
		}
	} nmeaPosition;

	struct
	{
		float speedKnots;
		float courseDegrees;
		float eastVelocityMps;
		float northVelocityMps;
		float upVelocityMps;
		
		const float knots2m = 0.514;
		void VelocityCalc()
		{
			float fi = courseDegrees * M_PI / 180;
			eastVelocityMps = speedKnots * sin(fi) * knots2m;
			northVelocityMps = speedKnots * cos(fi) * knots2m;
			upVelocityMps = 0;
		}
	} nmeaVelocity;

	struct
	{
		u8 dimension;
		u8 numberSv;
		u8 svprn[12];
		float pDop;
		float hDop;
		float vDop;
		float tDop;
		
		void PrecisionCalc()
		{
			dimension &= 0x0F;
			dimension |= numberSv<<4;
			tDop = 0;
		}
	} nmeaPrecision;

	struct 
	{
		u8 qualityIndicator; // 0 = No GPS, 1 = GPS, 2 = DGPS
		u8 nSatellitesUse;
		bool isTimeValid;
		u8 statusCode;
		
		void HealthClear()
		{
			qualityIndicator = 0;
			nSatellitesUse = 0;
			isTimeValid = 0;
		}
		
		void HealthCalc()
		{
			if(!isTimeValid)
			{
				statusCode = 0x01; //Don't have GPS time yet
			}
			else if(nSatellitesUse < 4)
			{
				statusCode = 0x08 + nSatellitesUse;
			}
			else statusCode = 0x00;
		}
		
	} nmeaHealth;

	void GetFixedDigits(s32 &param, u8 &divisor, u8 first6, u8 size) //Чтение чисел с фиксированной целой частью
	{
		if(charPoint == size) return;
		if(charPoint == 0)
		{
			param = 0;
			divisor = 0;
		}
		if(charPoint < size || divisor < MaxDecimalDivisor)
		{
			param *= (charPoint < size && (charPoint == first6 || charPoint == first6+2))?6:10;
			param += Hex2Int();
			if(charPoint > size) ++divisor;
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
					float temp = Hex2Int();
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
					param -= Hex2Int();
				}
				else
				{
					param += Hex2Int();
				}
			}
			break;
		}
	}
	
	void GetTime()
	{
		switch(charPoint)
		{
			case 7:
			nmeaDateTime.centiSeconds = Hex2Int()*10;
			break;
			case 8:
			nmeaDateTime.centiSeconds += Hex2Int();
			break;
			case 5:
			updateFlag |= UpdateTime;
			default:
			GetFixedDigits(nmeaDateTime.seconds, nmeaDateTime.centiSeconds, 2, 6);
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
			nmeaDateTime.day = Hex2Int();
			break;
			case 1:
			nmeaDateTime.day *= 10;
			nmeaDateTime.day += Hex2Int();
			break;
			case 2:
			nmeaDateTime.month = Hex2Int();
			break;
			case 3:
			nmeaDateTime.month *= 10;
			nmeaDateTime.month += Hex2Int();
			break;
			case 4:
			nmeaDateTime.year = Hex2Int();
			break;
			case 5:
			nmeaDateTime.year *= 10;
			nmeaDateTime.year += Hex2Int();
			
			if(nmeaDateTime.DateTimeCalc()) globalError = Error;
			updateFlag |= UpdateDate;
			break;
		}
	}

	void GetLatitude()
	{
		GetFixedDigits(nmeaPosition.latitudeMinutes, nmeaPosition.latitudeDivisor, 2, 4);
		if(charPoint == 3) updateFlag |= UpdateLatitude;
	}

	void GetLongitude()
	{
		GetFixedDigits(nmeaPosition.longitudeMinutes,nmeaPosition.longitudeDivisor, 3, 5);
		if(charPoint == 4) updateFlag |= UpdateLongitude;
	}
	
	void tsipPush(u8 c)
	{
		if(c == DLE) TsipPushRaw(DLE);
		TsipPushRaw(c);
	}
	

	void floatPush(float &arg)
	{
		u8 *f2byte = ((u8*) &arg) +3;
		for(u8 i = 0; i < 4; ++i, --f2byte)
		{
			tsipPush(*f2byte);
		}
	}
	void int16Push(u16 &arg)
	{
		u8 *i2byte = ((u8*) &arg) +1;
		for(u8 i = 0; i < 2; ++i, --i2byte)
		{
			tsipPush(*i2byte);
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
		if(byteCount < 6) checkSum ^= data;
		
		switch(++byteCount)
		{
			case 1: if(data!='$') byteCount=0; checkSum = 0; break;
			case 2: if(data!='G') byteCount=0; updateFlag = 0; break;
			case 3: if(data!='P' && data!='N') byteCount=0; break; //«GP» - GPS, «GN» - ГЛОНАСС+GPS
			case 4: if(data!='R' && data!='G') byteCount=0; break;
			case 5: msgType.msg8[0] = data; break;
			case 6: msgType.msg8[1] = data;
			comaPoint = 0;
			charPoint = 0;
			DebugPush('?');
			DebugPush(msgType.msg8[0]);
			DebugPush(msgType.msg8[1]);
			break;
			case 7:
			if(data != '*')
			{
				checkSum ^= data;
				byteCount = 6;
				if(data == ',')
				{
					++comaPoint;
					charPoint = 0;
					break;
				}
				switch(msgType.msg16)
				{
					case MSG_ENCODE('M','C'): //RMC - Рекомендованный минимальный набор GPS данных
					
					switch(comaPoint)
					{
						case 1: GetTime(); break; //UTC время
						case 2: nmeaHealth.isTimeValid = data == 'A'; break; //статус: «A» — данные достоверны, «V» — недостоверны.
						case 3: GetLatitude(); break; //широта
						case 4: if(data == 'S') nmeaPosition.latitudeMinutes *= -1; break; //«N» для северной или «S» для южной широты
						case 5: GetLongitude(); break; //долгота
						case 6: if(data == 'W') nmeaPosition.longitudeMinutes *= -1; break; //«E» для восточной или «W» для западной долготы
						case 7: GetFloat(nmeaVelocity.speedKnots); updateFlag |= UpdateSpeed; break; //скорость относительно земли в узлах
						case 8: GetFloat(nmeaVelocity.courseDegrees); updateFlag |= UpdateCourse; break; // путевой угол (направление скорости) в градусах по часовой от севера
						case 9: GetDate(); break; //дата
						case 10: break; //магнитное склонение в градусах
						case 11: break;//для получения магнитного курса «E» — вычесть, «W» — прибавить
						case 12: break;//индикатор режима: «A» — автономный, «D» — дифференциальный, «E» — аппроксимация, «N» — недостоверные данные
					}
					
					break;
					case MSG_ENCODE('G','A'): //GGA - данные о последнем определении местоположения
					switch(comaPoint)
					{
						case 1: GetTime(); break; //UTC время
						case 2:	GetLatitude(); break; //широта
						case 3:	if(data == 'S') nmeaPosition.latitudeMinutes *= -1; break; //«N» для северной или «S» для южной широты
						case 4:	GetLongitude(); break; //долгота
						case 5:	if(data == 'W') nmeaPosition.longitudeMinutes *= -1; break; //«E» для восточной или «W» для западной долготы
						case 6:	nmeaHealth.qualityIndicator = Hex2Int(); updateFlag |= UpdateQuality; break; //Качество фиксации позиции: 0 = No GPS, 1 = GPS, 2 = DGPS
						case 7:	nmeaHealth.nSatellitesUse = Hex2Int(); break; //количество используемых спутников
						case 8:	 break; //HDOP
						case 9:	GetFloat(nmeaPosition.mslAltitudeMeters); nmeaPosition.mslAboveHae = 0; updateFlag |= UpdateAltitude; break; //высота над уровнем моря
						case 10: break; //M - метры
						case 11: GetFloat(nmeaPosition.mslAboveHae); break; //высота уровня моря над эллипсоидом WGS 84
						case 12: break; //M - метры
						case 13: break; //время, прошедшее с момента получения последней DGPS поправки
						case 14: break; //идентификационный номер базовой станции DGPS
					}
					break;
					case MSG_ENCODE('S','A'): //GSA - общая информация о спутниках.
					switch(comaPoint)
					{
						case 1: nmeaPrecision.dimension = (data == 'M')?_BV(3):0; updateFlag |= UpdateDimension;  break;//выбора между 2D и 3D: A - автоматический, M - ручной
						case 2:	nmeaPrecision.dimension += (Hex2Int()==3)?4:3; break;//режим: 1 = данные не доступны, 2 = 2D, 3 = 3D
						
						case 15: GetFloat(nmeaPrecision.pDop); updateFlag |= UpdatePDop;	 break;//PDOP
						case 16: GetFloat(nmeaPrecision.hDop); updateFlag |= UpdateHDop;	 break;//HDOP
						case 17: GetFloat(nmeaPrecision.vDop); updateFlag |= UpdateVDop;	 break;//VDOP
						
						case 3: nmeaPrecision.numberSv = 0; updateFlag |= UpdatePrn; //PRN коды используемых в подсчете позиции спутников (12 полей)
						default: if(comaPoint < 15)	 
						{
							if(charPoint == 0) nmeaPrecision.svprn[nmeaPrecision.numberSv] = Hex2Int()<<4;
							else nmeaPrecision.svprn[nmeaPrecision.numberSv++] |= Hex2Int();
						}
						break;
					}
					break;
					case MSG_ENCODE('S','V'):
					break;
				}
				++charPoint;
			}
			break;
			case 8:
			checkSum ^= Hex2Int() << 4;
			break;
			case 9:
			checkSum ^= Hex2Int();
			if(checkSum) //Ошибка контрольной суммы
			{
				byteCount = 0;
				DebugPush(0xEE);
				DebugPush(0xCC);
				DebugPush(checkSum);
			}
			break;
			default: //Передача TSIP
			byteCount = 0;
			
			DebugPush('P');
			u16 tempTime = ppsTimeMSec;
			DebugPush(ppsTimeSec);
			DebugPush(tempTime>>8);
			DebugPush(tempTime);
			
			if((updateFlag & UpdateDateTime) == UpdateDateTime) //0x41 GPS Время
			{
				LED_PORT ^= LED_PIN;
				TsipPushRaw(DLE);
				TsipPushRaw(0x41);
				floatPush(nmeaDateTime.gpsTimeOfWeek);
				int16Push(nmeaDateTime.gpsWeekNumber);
				floatPush(nmeaDateTime.gpsUtcOffset);
				TsipPushRaw(DLE);
				TsipPushRaw(ETX);
			}
			if((updateFlag & UpdatePosition) == UpdatePosition) //0x4A Позиция
			{
				nmeaPosition.PositionCalc();
				nmeaDateTime.TimeOfFixCalc();
				TsipPushRaw(DLE);
				TsipPushRaw(0x4A);
				floatPush(nmeaPosition.latitudeRadians);
				floatPush(nmeaPosition.longitudeRadians);
				floatPush(nmeaPosition.haeAltitudeMeters);
				
				floatPush(Zero);
				floatPush(nmeaDateTime.timeOfFix);
				TsipPushRaw(DLE);
				TsipPushRaw(ETX);
			}
			if((updateFlag & UpdateVelocity) == UpdateVelocity) //0x56 Скорость
			{
				nmeaVelocity.VelocityCalc();
				nmeaDateTime.TimeOfFixCalc();
				TsipPushRaw(DLE);
				TsipPushRaw(0x56);
				floatPush(nmeaVelocity.eastVelocityMps);
				floatPush(nmeaVelocity.northVelocityMps);
				floatPush(nmeaVelocity.upVelocityMps);
				
				floatPush(Zero);
				floatPush(nmeaDateTime.timeOfFix);
				TsipPushRaw(DLE);
				TsipPushRaw(ETX);
			}
			if((updateFlag & UpdatePrecision) == UpdatePrecision) //0x6D Точность и PRN
			{
				if(!(updateFlag & UpdatePrn)) nmeaPrecision.numberSv = 0;
				nmeaPrecision.PrecisionCalc();
				
				TsipPushRaw(DLE);
				TsipPushRaw(0x6D);
				tsipPush(nmeaPrecision.dimension);
				floatPush(nmeaPrecision.pDop);
				floatPush(nmeaPrecision.hDop);
				floatPush(nmeaPrecision.vDop);
				floatPush(nmeaPrecision.tDop);
				for(u8 i = 0; i < nmeaPrecision.numberSv; i++)
				{ 
					tsipPush(nmeaPrecision.svprn[i]);
				}
				TsipPushRaw(DLE);
				TsipPushRaw(ETX);
			}
			if((updateFlag & UpdateQuality) == UpdateQuality) //0x82 Режим фиксации положения
			{
				TsipPushRaw(DLE);
				TsipPushRaw(0x82);
				tsipPush((nmeaHealth.qualityIndicator == 2)?3:2);
				TsipPushRaw(DLE);
				TsipPushRaw(ETX);
			}
			if(isHealthSend)
			{
				nmeaHealth.HealthCalc();
				TsipPushRaw(DLE);
				TsipPushRaw(0x46); //0x46 Здоровье приемника
				tsipPush(nmeaHealth.statusCode);
				tsipPush(0x00);
				TsipPushRaw(DLE);
				TsipPushRaw(ETX);
				
				TsipPushRaw(DLE);
				TsipPushRaw(0x4B); //0x4B Дополнительный статус
				tsipPush(0x5A);
				tsipPush(0x00);
				tsipPush(0x01);
				TsipPushRaw(DLE);
				TsipPushRaw(ETX);
				nmeaHealth.HealthClear();
				isHealthSend = false;
			}
			
			
			DebugPush('P');
			tempTime = ppsTimeMSec;
			DebugPush(ppsTimeSec);
			DebugPush(tempTime>>8);
			DebugPush(tempTime);
			break;
		}
		if(globalError == Error) 
		{
			byteCount = 0;
			DebugPush(0xEE);
			DebugPush(0x00);
			DebugPush(0xAE);
		}
		return globalError;
	}
	
};

#endif /*NMEA_PARSER_H_*/