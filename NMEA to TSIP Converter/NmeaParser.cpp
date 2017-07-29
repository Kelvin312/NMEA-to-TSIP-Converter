#ifndef NMEA_PARSER_H_
#define NMEA_PARSER_H_
#include "stdafx.h"


enum class ErrorCode:u8
{
	Ok = 0,
	Error,
	CheckSumError,
	NoData
};

class NmeaParser
{
	public: //Преобразование строки символов в хэш
	#define MSG_ENCODE(_p) u16(u16(*(_p)) << 10 ^ u16(*((_p)+1)) << 5 ^ u16(*((_p)+2)))
	
	ErrorCode result; //Результат разбора пакета NMEA
	void(*tsipPushRaw)(u8); //Функция отправки байта TSIP 
	
	static const float clockBiasConst; //Значение Clock Bias при включении питания
	static const float clockBiasRateConst; //Скорость изменения Clock Bias
	static const float gpsUtcOffsetConst; //Смещение времени GPS относительно UTC

	const u8 DLE = 0x10; //Байт DLE протокола TSIP
	const u8 ETX = 0x03; //Байт ETX протокола TSIP

	NmeaParser(void(*tsipPushRawA)(u8)) : tsipPushRaw(tsipPushRawA) //Функция отправки байта TSIP 
	{
	}

	u8 Hex2Int(u8 c) //Перевод ASCII в число / число / символ ASCII
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

	void GetFloat(float &param) //Перевод ASCII вещественного числа в число одинарной точности IEEE 754 / число одинарной точности IEEE 754
	{
		static bool isSign; //Флаг отрицательного числа
		static bool isDot; //Флаг точки
		static float divisor; //Делитель дробной части числа
		if (iCharCmd == 0)
		{
			isSign = false;
			isDot = false;
			param = 0;
			divisor = 1.0;
		}
		else if(!(isDigit(dataCmd) || (!isDot && dataCmd == '.')))
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
	} dataType = MsgStart; //Тип части пакета NMEA

	u8 iCmd, iCharCmd, dataCmd, checkSum; //Номер команды в пакете / номер символа в команде / символ команды / контрольная сумма
	u16 msgId; //Хэш ID пакета NMEA
	char msgName[3]; //ID пакета NMEA

	enum UpdateFlag //Флаги обновления данных
	{
		UpdateNone = 0,
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
		UpdatePrn = 128,
		UpdateQuality = 256
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
			case 9: result |= gpsTime.GetDate(iCharCmd, dataCmd, updateFlag); break; //дата ddmmyy
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
			case 6: result |= healthReceiver.GetQualityIndicator(dataCmd); updateFlag |= UpdateQuality; break; //Качество фиксации позиции: 0 = No GPS, 1 = GPS, 2 = DGPS
			case 7: result |= healthReceiver.GetNumberSv(iCharCmd, dataCmd); break; //количество используемых спутников
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
			case 1: satelliteView.GetSmode(dataCmd); break; //выбора между 2D и 3D: A - автоматический, M - ручной
			case 2: satelliteView.GetFixStatus(dataCmd);  //режим: 1 = данные не доступны, 2 = 2D, 3 = 3D
			satelliteView.numberSv = 0;  updateFlag |= UpdatePrn;
			break;

			case 15: GetFloat(satelliteView.pDop); break;//PDOP
			case 16: GetFloat(satelliteView.hDop); break;//HDOP
			case 17: GetFloat(satelliteView.vDop); break;//VDOP

			//PRN коды используемых в подсчете позиции спутников (12 полей)
			default: if (iCmd < 15) result |= satelliteView.GetSvPrn(iCharCmd, dataCmd);
			break;
		}
	}

	void Nmea2Tsip() //Вычисления для преобразования пакета NMEA в TSIP
	{
		if((updateFlag & UpdateDateTime) == UpdateDateTime)
		{
			gpsTime.DateTimeCalc();
		}
		if((updateFlag & UpdatePosition) == UpdatePosition)
		{
			llaPosition.PositionCalc();
			gpsTime.TimeOfFixCalc(llaPosition.timeOfFixSec);
		}
		if ((updateFlag & UpdateVelocity) == UpdateVelocity)
		{
			enuVelocity.VelocityCalc();
			gpsTime.TimeOfFixCalc(enuVelocity.timeOfFixSec);
		}
		if ((updateFlag & UpdatePrn) == UpdatePrn)
		{
			satelliteView.NumberSvCalc();
		}
		if ((updateFlag & UpdateQuality) == UpdateQuality)
		{
			healthReceiver.HealthCalc();
		}
	}

	ErrorCode Parse(u8 c) //Главная функция разбора пакетов NMEA
	{
		result = ErrorCode::Ok;
		if (c < 0x20 || c >= 0x7F)
		{
			if (dataType != MsgEnd && dataType != MsgStart)
			{
				result = ErrorCode::Error;
			}
			dataType = MsgStart;
			return result;
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
				updateFlag = UpdateNone;
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
			if (c != ',')
			{
				switch (msgId)
				{
					case MSG_ENCODE("RMC"): RmcParse(); break;
					case MSG_ENCODE("GGA"): GgaParse(); break;
					case MSG_ENCODE("GSA"): GsaParse(); break;
					default:;
				}
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
		const u8 size = 4 + 2 + 4; //Количество байт полей выше

		SGpsTime()
		{
			gpsTimeOfWeek = 0;
			gpsWeekNumber = 1958;
			gpsUtcOffset = gpsUtcOffsetConst;
			dayOfWeek = 0;
		}

		s32 second; //Секунд в текущем дне
		u8 centiSecond; //Сотых секунд
		u8 day; //Число
		u8 month; //Месяц
		u8 year; //Год
		u8 dayOfWeek; //День недели
		const u32 secInDay = u32(24) * 60 * 60; //Секунд в сутках

		void DateTimeCalc() //Вычисление времени недели и номера недели GPS
		{
			u16 m = month; //Месяц
			u16 y = year; //Год
			if (m > 2) { m -= 3; }
			else { m += 12 - 3; --y; }
			//https://ru.stackoverflow.com/a/455866
			//Количество дней с January 6, 1980
			u16 daysPassed = day + (153 * m + 2) / 5 + 365 * y + (y >> 2) + 7359; //- y / 100 + y / 400 - 723126;
			gpsWeekNumber = daysPassed / 7;
			dayOfWeek = daysPassed % 7;
			gpsTimeOfWeek = dayOfWeek*secInDay + second + float(centiSecond) / 100 + gpsUtcOffset;
			if (gpsTimeOfWeek >= 7 * secInDay)
			{
				gpsTimeOfWeek -= 7 * secInDay;
				gpsWeekNumber += 1;
			}
		}

		void DateTimeAdd(u8 sec) //Прибавление секунд к времени GPS / секунд
		{
			if (sec == 0) return;
			gpsTimeOfWeek += sec;
			if (gpsTimeOfWeek >= 7 * secInDay)
			{
				gpsTimeOfWeek -= 7 * secInDay;
				gpsWeekNumber += 1;
			}
		}

		void TimeOfFixCalc(float &timeOfFix) const //Вычисление GPS времени момента измерения / вычисленное GPS время 
		{
			timeOfFix = dayOfWeek*secInDay + second + float(centiSecond) / 100 + gpsUtcOffset;
			if (timeOfFix >= 7 * secInDay)
			{
				timeOfFix -= 7 * secInDay;
			}
		}

		ErrorCode GetTime(u8 iCharCmd, u8 c, UpdateFlag &flag) //hhmmss.ss // Перевод ASCII времени в секунды / код ошибки / номер символа в команде / символ команды / флаг обновления данных
		{
			if(!(isDigit(c) || (iCharCmd == 6 && c == '.')))
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

		ErrorCode GetDate(u8 iCharCmd, u8 c, UpdateFlag &flag) //ddmmyy // Перевод ASCII даты в год, месяц и число / код ошибки / номер символа в команде / символ команды / флаг обновления данных
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

	} gpsTime; //GPS время

	struct SLlaPosition //0x4A - LLA Position
	{
		float timeOfFixSec; //GPS время момента измерения
		float clockBiasMeters; //Clock Bias
		float haeAltitudeMeters; //Высота над эллипсойдом
		float longitudeRadians; //Долгота
		float latitudeRadians; //Широта
		const u8 size = 4 * 5; //Количество байт полей выше

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

		float RDCalc(const u16 divisor) const { return float(M_PI / 180 / 60 / divisor); } //Вычисление делителя для радиан / делитель / десятичный делитель
		const float RadiansDivisor[5] = { RDCalc(1),RDCalc(10),RDCalc(100),RDCalc(1000),RDCalc(10000) }; //Массив делителей для радиан
		const u8 MaxDivisor = 4; //Максимальный индекс делителя
		
		void PositionCalc() //Перевод минут в радианы и вычисление высоты над уровнем моря
		{
			latitudeRadians = latitudeMinutes * RadiansDivisor[latitudeDivisor];
			longitudeRadians = longitudeMinutes * RadiansDivisor[longitudeDivisor];
			haeAltitudeMeters = mslAltitudeMeters + mslAboveHae;
			clockBiasMeters += clockBiasRateConst;
		}

		ErrorCode GetLatitude(u8 iCharCmd, u8 c, UpdateFlag &flag) //llmm.mmm // Перевод ASCII широты в минуты / код ошибки / номер символа в команде / символ команды / флаг обновления данных
		{
			if (!(isDigit(c) || (iCharCmd == 4 && c == '.')))
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

		ErrorCode GetLongitude(u8 iCharCmd, u8 c, UpdateFlag &flag) //yyymm.mmm // Перевод ASCII долготы в минуты / код ошибки / номер символа в команде / символ команды / флаг обновления данных
		{
			if (!(isDigit(c) || (iCharCmd == 5 && c == '.')))
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

	} llaPosition; //Позиция

	struct SEnuVelocity //0x56 - ENU Velocity
	{
		float timeOfFixSec; //GPS время момента измерения
		float clockBiasRateMps; //Скорость изменения Clock Bias
		float upVelocityMps; //Скорость вверх
		float northVelocityMps; //Скорость на север
		float eastVelocityMps; //Скорость на восток
		const u8 size = 4 * 5; //Количество байт полей выше

		SEnuVelocity()
		{
			timeOfFixSec = 0;
			clockBiasRateMps = clockBiasRateConst;
			upVelocityMps = 0;
			northVelocityMps = 0;
			eastVelocityMps = 0;
		}

		float speedKnots; //Скорость в узлах
		float courseDegrees; //Путевой угол в градусах
		const float knots2m = 0.514f; //Отношение узлов к метрам

		void VelocityCalc() //Перевод полярных скоростей в декартовые с переводом узлов в метры
		{
			float fi = courseDegrees * float(M_PI / 180); //Путевой угол в радианах
			float speedMps = speedKnots * knots2m; //Скорость в метрах
			eastVelocityMps = speedMps * sin(fi);
			northVelocityMps = speedMps * cos(fi);
		}

	} enuVelocity; //Скорость

	struct SSatelliteView //0x6D - All-In-View Satellite Selection
	{
		float tDop; //Снижение точности по времени
		float vDop; //Снижение точности в вертикальной плоскости
		float hDop; //Снижение точности в горизонтальной плоскости
		float pDop; //Снижение точности по местоположению
		u8 dimension; //Количество используемых спутников, 2D или 3D и автоматический или ручной режим
		const u8 size = 4 * 4 + 1; //Количество байт полей выше
		u8 svPrn[12]; //ID спутников

		SSatelliteView()
		{
			dimension = 3;
			tDop = 1.0;
			vDop = 99.0;
			hDop = 99.0;
			pDop = 99.0;
			numberSv = 0;
		}

		u8 numberSv; //Количество спутников

		void NumberSvCalc() //Занесение количества спутников в dimension
		{
			dimension &= 0x0F;
			dimension |= numberSv << 4;
		}

		ErrorCode GetSvPrn(u8 iCharCmd, u8 c) //Перевод ASCII ID спутника в число / код ошибки / номер символа в команде / символ команды
		{
			if (!isDigit(c)) return ErrorCode::Error;
			if (iCharCmd == 0)
			{
				++numberSv;
				svPrn[numberSv - 1] = 0;
			}
			svPrn[numberSv - 1] *= 10;
			svPrn[numberSv - 1] += Dec2Int(c);
			return ErrorCode::Ok;
		}

		ErrorCode GetSmode(u8 c) //Чтение режима / код ошибки / символ команды
		{
			dimension &= 0xF7;
			if (c == 'M') dimension |= _BV(3);
			return ErrorCode::Ok;
		}

		ErrorCode GetFixStatus(u8 c) //Чтение статуса / код ошибки / символ команды
		{
			if (!isDigit(c)) return ErrorCode::Error;
			dimension &= 0xF8;
			dimension |= (c == '3') ? 4 : 3;
			return ErrorCode::Ok;
		}

	} satelliteView; //Информация о спутниках и снижении точности

	struct SHealthReceiver //0x46 - Health of Receiver
	{
		u8 errorCode; //Код аппаратных ошибок
		u8 statusCode; //Код статуса
		const u8 size = 2; //Количество байт полей выше

		SHealthReceiver()
		{
			errorCode = 0;
			statusCode = 1;
			qualityIndicator = 0;
		}

		u8 numberSv; //Количество спутников
		u8 qualityIndicator; //Индикатор качества

		void HealthCalc() //Вычисление кода статуса
		{
			if(qualityIndicator == 0)
			{
				statusCode = 1;
			}
			else if(numberSv < 4)
			{
				statusCode = 0x08 + numberSv;
			}
			else
			{
				statusCode = 0;
			}
		}

		ErrorCode GetQualityIndicator(u8 c) //Чтение индикатора качества / код ошибки / символ команды
		{
			if (!isDigit(c)) return ErrorCode::Error;
			qualityIndicator = Dec2Int(c);
			return ErrorCode::Ok;
		}

		ErrorCode GetNumberSv(u8 iCharCmd, u8 c) //Чтение количества спутников / код ошибки / номер символа в команде / символ команды
		{
			if (!isDigit(c)) return ErrorCode::Error;
			if (iCharCmd == 0) numberSv = 0;
			numberSv *= 10;
			numberSv += Dec2Int(c);
			return ErrorCode::Ok;
		}

	} healthReceiver; //Здоровье приемника

	#pragma pack(pop)
	
	//_TsipSend
	#pragma region _TsipSend
	
	void TsipPushDle(u8 c) const //Отправка заголовка пакета TSIP
	{
		tsipPushRaw(DLE);
		tsipPushRaw(c);
	}
	void TsipPush(u8 c) const //Отправка данных пакета TSIP
	{
		if (c == DLE) tsipPushRaw(DLE);
		tsipPushRaw(c);
	}
	void TsipPushDleEtx() const //Отправка хвоста пакета TSIP
	{
		tsipPushRaw(DLE);
		tsipPushRaw(ETX);
	}

	void TsipPayload(void *sptr, u8 size) const //Отправка структуры с данными пакета TSIP / указатель на структуру / количество отправляемых байт
	{
		u8 *ptr = static_cast<u8*>(sptr); //указатель на структуру
		ptr += size;
		while(size--)
		{
			--ptr;
			TsipPush(*ptr);
		}
	}

	void PositionAndVelocitySend() //Отправка положения и скорости
	{
		TsipPushDle(0x4A); //0x4A Позиция
		TsipPayload(&llaPosition, llaPosition.size);
		TsipPushDleEtx();

		TsipPushDle(0x56); //0x56 Скорость
		TsipPayload(&enuVelocity, enuVelocity.size);
		TsipPushDleEtx();
	}

	void GpsTimeSend() //Отправка GPS времени
	{
		TsipPushDle(0x41); //0x41 GPS Время
		TsipPayload(&gpsTime, gpsTime.size);
		TsipPushDleEtx();
	}

	void HealthSend() //Отправка здоровья приемника и дополнительного статуса
	{
		TsipPushDle(0x46); //0x46 Здоровье приемника
		TsipPayload(&healthReceiver, healthReceiver.size);
		TsipPushDleEtx();

		TsipPushDle(0x4B); //0x4B Дополнительный статус
		TsipPush(0x5A);
		TsipPush(0x00);
		TsipPush(0x01);
		TsipPushDleEtx();
	}

	void SatelliteViewSend() //Отправка снижения точности, ID спутников и режима фиксации
	{
		TsipPushDle(0x6D); //0x6D Точность и PRN
		TsipPayload(&satelliteView, satelliteView.size);
		for (u8 i = 0; i < satelliteView.numberSv; i++)
		{
			TsipPush(satelliteView.svPrn[i]); //PRN спутников
		}
		TsipPushDleEtx();

		TsipPushDle(0x82); //0x82 Режим фиксации положения
		TsipPush((healthReceiver.qualityIndicator == 2) ? 3 : 2); //DGPS/GPS
		TsipPushDleEtx();
	}
	
	#pragma endregion _TsipSend
};


#endif /*NMEA_PARSER_H_*/