#ifndef STDAFX_H_
#define STDAFX_H_

#define F_CPU 16000000UL  // 16 MHz //Частота тактирования МК
#define LED_PIN _BV(5)  //Пин отладочного светодиода
#define LED_PORT PORTB //Порт этого светодиода

#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/wdt.h>
#include <avr/power.h>
#include <avr/sleep.h>
#include <avr/eeprom.h>
#include <avr/pgmspace.h> 

#include <string.h>
#include <math.h>

typedef unsigned char u8;
typedef signed char s8;
typedef unsigned short u16;
typedef signed short s16;
typedef unsigned long u32;
typedef signed long s32;

template<class T> inline constexpr T operator~ (T a) { return static_cast<T>(~static_cast<int>(a)); }
template<class T> inline constexpr T operator| (T a, T b) { return static_cast<T>(static_cast<int>(a) | static_cast<int>(b)); }
template<class T> inline constexpr T operator& (T a, T b) { return static_cast<T>(static_cast<int>(a) & static_cast<int>(b)); }
template<class T> inline constexpr T operator^ (T a, T b) { return static_cast<T>(static_cast<int>(a) ^ static_cast<int>(b)); }
template<class T> inline T& operator|= (T& a, T b) { return reinterpret_cast<T&>(reinterpret_cast<int&>(a) |= static_cast<int>(b)); }
template<class T> inline T& operator&= (T& a, T b) { return reinterpret_cast<T&>(reinterpret_cast<int&>(a) &= static_cast<int>(b)); }
template<class T> inline T& operator^= (T& a, T b) { return reinterpret_cast<T&>(reinterpret_cast<int&>(a) ^= static_cast<int>(b)); }

inline bool isDigit(u8 c) //Цифра ли это / ответ / символ
{
	return c >= '0' && c <= '9';
}

inline u8 Dec2Int(u8 c) //Преобразование символа в цифру / цифра / символ
{
	return c - '0';
}

//Шаблонная магия для задания типа по булеву флагу
// TEMPLATE CLASS conditional
template<bool _Test,
class _Ty1,
class _Ty2>
struct conditional
{	// type is _Ty2 for assumed !_Test
	typedef _Ty2 type;
};

template<class _Ty1,
class _Ty2>
struct conditional<true, _Ty1, _Ty2>
{	// type is _Ty1 for _Test
	typedef _Ty1 type;
};

//Кольцевой буфер
template<u16 S> struct RingBuffer //Размер буфера в байтах
{
	private:
	typedef typename conditional<(S>128), u16, u8 >::type T;
	static const u16 S0 = S-1, S1 = S0 | S0 >> 1, S2 = S1 | S1 >> 2, S4 = S2 | S2 >> 4, S8 = S4 | S4 >> 8; //Число степени двойки, что-бы поместился буфер

	static const T bufferSize = S8 + 1; //Размер буфера
	volatile T writeIndex = 0, readIndex = 0; //Индексы записи и чтения
	volatile u8 buffer[bufferSize]; //Сам буфер

	public:
	volatile bool isOverflow = false; //Фалг переполнения буфера

	void Push(u8 data) //Вставить в буфер / байт
	{
		if (Size() == bufferSize) //Переполнение
		{
			isOverflow = true;
			return;
		}
		buffer[writeIndex++ & (bufferSize - 1)] = data;
	}
	u8 Pop() //Взять из буфера байт и удалить его / байт
	{
		if (Empty()) return 0; //Буфер пустой
		return buffer[readIndex++  & (bufferSize - 1)];
	}
	u8 Front() const //Взять из начала буфера байт / байт
	{
		if (Empty()) return 0;
		return buffer[readIndex & (bufferSize - 1)];
	}
	u8 Back() const //Взять из конца буфера байт / байт
	{
		if (Empty()) return 0;
		return buffer[writeIndex - 1 & (bufferSize - 1)];
	}
	void Clear() //Очистить буфер
	{
		readIndex = writeIndex = 0;
	}
	T Size() const //Занятый размер буфера / размер буфера
	{
		return writeIndex - readIndex;
	}
	bool Empty() const //Пустой ли буфер / флаг пустого буфера
	{
		return writeIndex == readIndex;
	}
};


enum class ParityAndStop:u8
{
	None1 =  0,
	//None2 =  1,
	Even1 = 4,
	//Even2 = 5,
	Odd1 = 6,
	//Odd2 = 7
};

#endif /* STDAFX_H_ */