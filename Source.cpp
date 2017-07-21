#define _BV(_b) (1<<(_b))
#define M_PI 3.1416

#define F_CPU 16000000UL  // 16 MHz
#define LED_PIN _BV(5)
#define LED_PORT PORTB

#include <string.h>
#include <math.h>

typedef unsigned char u8;
typedef signed char s8;
typedef unsigned short u16;
typedef signed short s16;
typedef unsigned long u32;
typedef signed long s32;


template<class T> inline T operator~ (T a) { return static_cast<T>(~static_cast<int>(a)); }
template<class T> inline T operator| (T a, T b) { return static_cast<T>(static_cast<int>(a) | static_cast<int>(b)); }
template<class T> inline T operator& (T a, T b) { return static_cast<T>(static_cast<int>(a) & static_cast<int>(b)); }
template<class T> inline T operator^ (T a, T b) { return static_cast<T>(static_cast<int>(a) ^ static_cast<int>(b)); }
template<class T> inline T& operator|= (T& a, T b) { return static_cast<T&>(static_cast<int&>(a) |= static_cast<int>(b)); }
template<class T> inline T& operator&= (T& a, T b) { return static_cast<T&>(static_cast<int&>(a) &= static_cast<int>(b)); }
template<class T> inline T& operator^= (T& a, T b) { return static_cast<T&>(static_cast<int&>(a) ^= static_cast<int>(b)); }

inline bool isDigit(u8 c)
{
	return c >= '0' && c <= '9';
}

inline u8 Dec2Int(u8 c)
{
	return c - '0';
}

int main()
{
	return 0;
}


enum class ErrorCode:u8
{
	Ok = 0,
	Error,
	CheckSumError,
	NoData
};

class NmeaParser
{
public:
#define MSG_ENCODE(_p) u16(u16(*(_p)) << 10 ^ u16(*((_p)+1)) << 5 ^ u16(*((_p)+2)))
	
	ErrorCode result;
	void(*tsipPushRaw)(u8);
	
	static const float clockBiasConst;
	static const float clockBiasRateConst;
	static const float gpsUtcOffsetConst;

	NmeaParser(void(*tsipPushRawA)(u8)) : tsipPushRaw(tsipPushRawA)
	{
	}

	u8 Hex2Int(u8 c) //Перевод ASCII в число
	{
		if (isDigit(c))
			return c - '0';
		if (c >= 'A' && c <= 'F')
			return c - 'A' + 10;
		if (c >= 'a' && c <= 'f')
			return c - 'a' + 10;
		result = ErrorCode::Error;
		return 0;
	} 

	void GetFloat(float &param)
	{
		static bool isSign;
		static bool isDot;
		static float divisor;
		if (iCharCmd == 0)
		{
			isSign = false;
			isDot = false;
			param = 0;
			divisor = 1.0;
		}
		else if(!(isDigit(dataCmd) || !isDot && dataCmd == '.'))
		{
			result = ErrorCode::Error;
			return;
		}

		switch (dataCmd)
		{
		case ' ':
		case '+': break;
		case '-': isSign = true; break;
		case '.': isDot = true; break;
		default:
			if (!isDigit(dataCmd))
			{
				result = ErrorCode::Error;
				return;
			}
			float temp = Dec2Int(dataCmd);
			if (isDot)
			{
					divisor *= 10;
					temp /= divisor;
			}
			else
			{
				param *= 10;
			}
			if (isSign) param -= temp;
			else param += temp;
			break;
		}
	}

	enum
	{
		MsgStart,
		MsgID,
		MsgData,
		MsgCSh,
		MsgCSl,
		MsgEnd
	} dataType = MsgStart;

	u8 iCmd, iCharCmd, dataCmd, checkSum;
	u16 msgId;
	char msgName[3];

	enum UpdateFlag //Флаги обновления данных
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
		UpdateVelocity = UpdateSpeed,
		UpdateDimension = 128,
		UpdatePrn = 256,
		UpdatePDop = 512,
		UpdateHDop = 1024,
		UpdateVDop = 2048,
		UpdatePrecision = UpdateDimension | UpdatePDop | UpdateHDop | UpdateVDop,
		UpdateQuality = 4096
	} updateFlag;

	void RmcParse() //RMC - Рекомендованный минимальный набор GPS данных
	{
		switch (iCmd) //RMC,hhmmss,status,latitude,N,longitude,E,spd,cog,ddmmyy,mv,mvE,mode
		{
		case 1: result |= gpsTime.GetTime(iCharCmd, dataCmd, updateFlag); break; //UTC время hhmmss.ss 
		case 2: break; //статус: «A» — данные достоверны, «V» — недостоверны.
		case 3: result |= llaPosition.GetLatitude(iCharCmd, dataCmd, updateFlag); break; //широта
		case 4: if (dataCmd == 'S') llaPosition.latitudeMinutes *= -1; break; //«N» для северной или «S» для южной широты
		case 5: result |= llaPosition.GetLongitude(iCharCmd, dataCmd, updateFlag); break; //долгота
		case 6: if (dataCmd == 'W') llaPosition.longitudeMinutes *= -1; break; //«E» для восточной или «W» для западной долготы
		case 7: GetFloat(enuVelocity.speedKnots); enuVelocity.courseDegrees = 0; updateFlag |= UpdateSpeed; break; //скорость относительно земли в узлах
		case 8: GetFloat(enuVelocity.courseDegrees); updateFlag |= UpdateCourse; break; // путевой угол (направление скорости) в градусах по часовой от севера
		case 9: gpsTime.GetDate(iCharCmd, dataCmd, updateFlag); break; //дата ddmmyy
		case 10: break; //магнитное склонение в градусах
		case 11: break; //для получения магнитного курса «E» — вычесть, «W» — прибавить
		case 12: break; //индикатор режима: «A» — автономный, «D» — дифференциальный, «E» — аппроксимация, «N» — недостоверные данные
		}
	}

	void GgaParse() // GGA - данные о последнем определении местоположения
	{
		switch (iCmd) //GGA,hhmmss.ss,Latitude,N,Longitude,E,FS,NoSV,HDOP,msl,m,Altref,m,DiffAge,DiffStation
		{
		case 1: result |= gpsTime.GetTime(iCharCmd, dataCmd, updateFlag); break; //UTC время hhmmss.ss
		case 2:	result |= llaPosition.GetLatitude(iCharCmd, dataCmd, updateFlag); break; //широта
		case 3:	if (dataCmd == 'S') llaPosition.latitudeMinutes *= -1; break; //«N» для северной или «S» для южной широты
		case 4:  result |= llaPosition.GetLongitude(iCharCmd, dataCmd, updateFlag); break; //долгота
		case 5:	if (dataCmd == 'W') llaPosition.longitudeMinutes *= -1; break; //«E» для восточной или «W» для западной долготы
		case 6://	nmeaHealth.qualityIndicator = Hex2Int(); updateFlag |= UpdateQuality; break; //Качество фиксации позиции: 0 = No GPS, 1 = GPS, 2 = DGPS
		case 7://	nmeaHealth.numberSv = Hex2Int(); break; //количество используемых спутников
		case 8:	 break; //HDOP
		case 9:	GetFloat(llaPosition.mslAltitudeMeters); llaPosition.mslAboveHae = 0; updateFlag |= UpdateAltitude; break; //высота над уровнем моря
		case 10: break; //M - метры
		case 11: GetFloat(llaPosition.mslAboveHae); break; //высота уровня моря над эллипсоидом WGS 84
		case 12: break; //M - метры
		case 13: break; //время, прошедшее с момента получения последней DGPS поправки
		case 14: break; //идентификационный номер базовой станции DGPS
		}
	}

	void GsaParse() //GSA - общая информация о спутниках.
	{
		switch (iCmd) //GSA,Smode,FS{,sv},PDOP,HDOP,VDOP
		{
		case 1: // satelliteView.dimension = dataCmd == 'M' ? _BV(3) : 0; updateFlag |= UpdateDimension;  break;//выбора между 2D и 3D: A - автоматический, M - ручной
		case 2:	// satelliteView.dimension += Dec2Int(dataCmd) == 3 ? 4 : 3; break;//режим: 1 = данные не доступны, 2 = 2D, 3 = 3D

		case 15: GetFloat(satelliteView.pDop); updateFlag |= UpdatePDop; break;//PDOP
		case 16: GetFloat(satelliteView.hDop); updateFlag |= UpdateHDop; break;//HDOP
		case 17: GetFloat(satelliteView.vDop); updateFlag |= UpdateVDop; break;//VDOP

		case 3: satelliteView.numberSv = 0; updateFlag |= UpdatePrn; //PRN коды используемых в подсчете позиции спутников (12 полей)
		default: if (iCmd < 15) result |= satelliteView.GetSvPrn(iCharCmd, dataCmd); 
		break;
		}
	}

	void Nmea2Tsip()
	{
		
	}

	ErrorCode Parse(u8 c)
	{
		result = ErrorCode::Ok;
		if (c < 0x20 || c >= 0x7F)
		{
			if (dataType != MsgEnd && dataType != MsgStart)
			{
				result = ErrorCode::Error;
			}
			dataType = MsgStart;
		}
		switch (dataType)
		{
		case MsgStart:
			if (c == '$')
			{
				dataType = MsgID;
				iCmd = 0;
				iCharCmd = -1;
				checkSum = 0;
			}
			break;
		case MsgID:
			switch (iCharCmd)
			{
			case 0: if (c != 'G')  dataType = MsgStart; break;
			case 1: if (c != 'P' && c != 'N') dataType = MsgStart; break; //«GP» - GPS, «GN» - ГЛОНАСС+GPS
			default:
				if (c < 'A' || c > 'Z')
				{
					dataType = MsgStart;
					break;
				}
				msgName[iCharCmd - 2] = c;
				if (iCharCmd == 4)
				{
					msgId = MSG_ENCODE(msgName);
					dataType = MsgData;
				}
				break;
			}
			checkSum ^= c;
			break;
		case MsgData:
			if (c == '*')
			{
				dataType = MsgCSh;
				break;
			}
			checkSum ^= c;
			dataCmd = c;
			switch (msgId)
			{
			case MSG_ENCODE("RMC"): RmcParse(); break;
			case MSG_ENCODE("GGA"): GgaParse(); break;
			case MSG_ENCODE("GSA"): GsaParse(); break;
			default: ;
			}
			break;
		case MsgCSh:
			dataType = MsgCSl;
			checkSum ^= Hex2Int(c) << 4;
			break;
		case MsgCSl:
			dataType = MsgEnd;
			checkSum ^= Hex2Int(c);
			if(checkSum == 0)
			{
				Nmea2Tsip();
			}
			else result = ErrorCode::CheckSumError;
			break;
		case MsgEnd: break;
		default: ;
		}

		++iCharCmd;
		if (c == ',')
		{
			++iCmd;
			iCharCmd = 0;
		}
		if (result != ErrorCode::Ok) dataType = MsgStart;
		return result;
	}



#pragma pack(push, 1)

	struct SGpsTime //0x41 - GPS Time
	{
		float gpsUtcOffset; //Смещение времени GPS UTC секунд
		s16 gpsWeekNumber; //Расширенный номер текущей GPS недели
		float gpsTimeOfWeek; //Секунд в текущей GPS неделе
		const u8 size = 4 + 2 + 4;

		SGpsTime()
		{
			gpsTimeOfWeek = 0;
			gpsWeekNumber = 1958;
			gpsUtcOffset = gpsUtcOffsetConst;
			dayOfWeek = 0;
		}

		s32 second; //Секунд в текущем дне
		u8 centiSecond; //Сотых секунд
		u8 day;
		u8 month;
		u8 year;
		u8 dayOfWeek;
		const u32 secInDay = u32(24) * 60 * 60; //Секунд в сутках 

		void DateTimeCalc() //Вычисление времени недели и номера недели GPS
		{
			u16 m = month;
			u16 y = year;
			if (m > 2) { m -= 3; }
			else { m += 12 - 3; --y; }
			//https://ru.stackoverflow.com/a/455866
			//Количество дней с January 6, 1980
			u16 daysPassed = day + (153 * m + 2) / 5 + 365 * y + (y >> 2) + 7359; //- y / 100 + y / 400 - 723126;
			gpsWeekNumber = daysPassed / 7;
			dayOfWeek = daysPassed % 7;
			gpsTimeOfWeek = dayOfWeek*secInDay + second + float(centiSecond) / 100.0 + gpsUtcOffset;
			if (gpsTimeOfWeek >= 7 * secInDay)
			{
				gpsTimeOfWeek -= 7 * secInDay;
				gpsWeekNumber += 1;
			}
		}

		void DateTimeAdd(u8 sec)
		{
			if (sec == 0) return;
			gpsTimeOfWeek += sec;
			if (gpsTimeOfWeek >= 7 * secInDay)
			{
				gpsTimeOfWeek -= 7 * secInDay;
				gpsWeekNumber += 1;
			}
		}

		void TimeOfFixCalc(float &timeOfFix) const
		{
			timeOfFix = dayOfWeek*secInDay + second + float(centiSecond) / 100.0 + gpsUtcOffset;
			if (timeOfFix >= 7 * secInDay)
			{
				timeOfFix -= 7 * secInDay;
			}
		}

		ErrorCode GetTime(u8 iCharCmd, u8 c, UpdateFlag &flag) //hhmmss.ss
		{
			if(!(isDigit(c) || iCharCmd == 6 && c == '.'))
			{
				return ErrorCode::Error;
			}
			switch (iCharCmd)
			{
			case 0: 
				second = Dec2Int(c);
				centiSecond = 0;
				break;
			case 5:
				flag |= UpdateTime;
			case 1:
			case 3:
				second *= 10;
				second += Dec2Int(c);
				break;
			case 2:
			case 4:
				second *= 6;
				second += Dec2Int(c);
				break;
			case 7:
				centiSecond = Dec2Int(c) * 10;
				break;
			case 8:
				centiSecond += Dec2Int(c);
				break;
			default: ;
			}
			return ErrorCode::Ok;
		}

		ErrorCode GetDate(u8 iCharCmd, u8 c, UpdateFlag &flag) //ddmmyy
		{
			if (!isDigit(c))
			{
				return ErrorCode::Error;
			}
			switch (iCharCmd)
			{
			case 0:
				month = 0;
				year = 0;
				day = Dec2Int(c);
				break;
			case 1:
				day *= 10;
				day += Dec2Int(c);
				break;
			case 2:
				month = Dec2Int(c);
				break;
			case 3:
				month *= 10;
				month += Dec2Int(c);
				break;
			case 4:
				year = Dec2Int(c);
				break;
			case 5:
				year *= 10;
				year += Dec2Int(c);
				flag |= UpdateDate;
				break;
			default: return ErrorCode::Error;
			}
			return ErrorCode::Ok;
		}

	} gpsTime;

	struct SLlaPosition //0x4A - LLA Position
	{
		float timeOfFixSec;
		float clockBiasMeters;
		float haeAltitudeMeters;
		float longitudeRadians;
		float latitudeRadians;
		const u8 size = 4 * 5;

		SLlaPosition()
		{
			timeOfFixSec = 0;
			clockBiasMeters = clockBiasConst;
			haeAltitudeMeters = 0;
			longitudeRadians = 0;
			latitudeRadians = 0;
			mslAltitudeMeters = 0;
			mslAboveHae = 0;
		}

		s32 latitudeMinutes;
		u8 latitudeDivisor;
		s32 longitudeMinutes;
		u8 longitudeDivisor;
		float mslAltitudeMeters;
		float mslAboveHae;

		float RDCalc(const u16 divisor) const { return M_PI / 180 / 60 / divisor; }
		const float RadiansDivisor[5] = { RDCalc(1),RDCalc(10),RDCalc(100),RDCalc(1000),RDCalc(10000) };
		const u8 MaxDivisor = 4;
		
		void PositionCalc()
		{
			latitudeRadians = latitudeMinutes * RadiansDivisor[latitudeDivisor];
			longitudeRadians = longitudeMinutes * RadiansDivisor[longitudeDivisor];
			haeAltitudeMeters = mslAltitudeMeters + mslAboveHae;
			clockBiasMeters += clockBiasRateConst;
		}

		ErrorCode GetLatitude(u8 iCharCmd, u8 c, UpdateFlag &flag) //llmm.mmm
		{
			if (!(isDigit(c) || iCharCmd == 4 && c == '.'))
			{
				return ErrorCode::Error;
			}
			switch (iCharCmd)
			{
			case 0:
				latitudeMinutes = Dec2Int(c);
				latitudeDivisor = 0;
				break;
			case 3:
				flag |= UpdateLatitude;
			case 1:
				latitudeMinutes *= 10;
				latitudeMinutes += Dec2Int(c);
				break;
			case 2:
				latitudeMinutes *= 6;
				latitudeMinutes += Dec2Int(c);
				break;
			case 4:
				break;
			default:
				if (latitudeDivisor < MaxDivisor)
				{
					latitudeMinutes *= 10;
					latitudeMinutes += Dec2Int(c);
					++latitudeDivisor;
				}
				break;
			}
			return ErrorCode::Ok;
		}

		ErrorCode GetLongitude(u8 iCharCmd, u8 c, UpdateFlag &flag) //yyymm.mmm
		{
			if (!(isDigit(c) || iCharCmd == 5 && c == '.'))
			{
				return ErrorCode::Error;
			}
			switch (iCharCmd)
			{
			case 0:
				longitudeMinutes = Dec2Int(c);
				longitudeDivisor = 0;
				break;
			case 4:
				flag |= UpdateLongitude;
			case 1:
			case 2:
				longitudeMinutes *= 10;
				longitudeMinutes += Dec2Int(c);
				break;
			case 3:
				longitudeMinutes *= 6;
				longitudeMinutes += Dec2Int(c);
				break;
			case 5:
				break;
			default:
				if (longitudeDivisor < MaxDivisor)
				{
					longitudeMinutes *= 10;
					longitudeMinutes += Dec2Int(c);
					++longitudeDivisor;
				}
				break;
			}
			return ErrorCode::Ok;
		}

	} llaPosition;

	struct SEnuVelocity //0x56 - ENU Velocity
	{
		float timeOfFixSec;
		float clockBiasRateMps;
		float upVelocityMps;
		float northVelocityMps;
		float eastVelocityMps;
		const u8 size = 4 * 5;

		SEnuVelocity()
		{
			timeOfFixSec = 0;
			clockBiasRateMps = clockBiasRateConst;
			upVelocityMps = 0;
			northVelocityMps = 0;
			eastVelocityMps = 0;
		}

		float speedKnots;
		float courseDegrees;
		const float knots2m = 0.514;

		void VelocityCalc()
		{
			float fi = courseDegrees * M_PI / 180;
			float speedMps = speedKnots * knots2m;
			eastVelocityMps = speedMps * sin(fi);
			northVelocityMps = speedMps * cos(fi);
		}

	} enuVelocity;

	struct SSatelliteView
	{
		float tDop;
		float vDop;
		float hDop;
		float pDop;
		u8 dimension;
		const u8 size = 4 * 4 + 1;
		u8 svPrn[12];

		SSatelliteView()
		{
			dimension = 3;
			tDop = 0;
			vDop = 0;
			hDop = 0;
			pDop = 0;
		}

		u8 numberSv;

		void SatelliteViewCalc()
		{
			dimension &= 0x0F;
			dimension |= numberSv << 4;
			tDop = 1.0;
		}

		ErrorCode GetSvPrn(u8 iCharCmd, u8 c)
		{
			if (!isDigit(c)) return ErrorCode::Error;
			if (iCharCmd == 0) svPrn[numberSv] = Dec2Int(c) << 4;
			else svPrn[numberSv++] |= Dec2Int(c);
			return ErrorCode::Ok;
		}

		ErrorCode GetSmode(u8 c)
		{
			
		}

		ErrorCode GetFixStatus(u8 c)
		{

		}


	} satelliteView;

#pragma pack(pop)
};

const float NmeaParser::clockBiasConst = -16000.0;
const float NmeaParser::clockBiasRateConst = 156.0;
const float NmeaParser::gpsUtcOffsetConst = 18.0;


