#ifndef STDAFX_H_
#define STDAFX_H_

#define F_CPU 16000000UL //Частота тактирования МК 16 МГц
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

//Шаблоны для применения побитовых операций к перечислениям
template<class T> inline constexpr T operator~ (T a) { return static_cast<T>(~static_cast<int>(a)); }
template<class T> inline constexpr T operator| (T a, T b) { return static_cast<T>(static_cast<int>(a) | static_cast<int>(b)); }
template<class T> inline constexpr T operator& (T a, T b) { return static_cast<T>(static_cast<int>(a) & static_cast<int>(b)); }
template<class T> inline constexpr T operator^ (T a, T b) { return static_cast<T>(static_cast<int>(a) ^ static_cast<int>(b)); }
template<class T> inline T& operator|= (T& a, T b) { return reinterpret_cast<T&>(reinterpret_cast<int&>(a) |= static_cast<int>(b)); }
template<class T> inline T& operator&= (T& a, T b) { return reinterpret_cast<T&>(reinterpret_cast<int&>(a) &= static_cast<int>(b)); }
template<class T> inline T& operator^= (T& a, T b) { return reinterpret_cast<T&>(reinterpret_cast<int&>(a) ^= static_cast<int>(b)); }

//Функция возвращает true, если символ является цифрой
//c: проверяемый символ
inline bool isDigit(u8 c) { return c >= '0' && c <= '9'; }

//Функция возвращает цифру, соответствующую десятичному символу
//c: символ
inline u8 Dec2Int(u8 c) { return c - '0'; }

//Шаблон, задающий тип type в зависимости от _Test
//Если _Test == false, то type будет типа _Ty2
template<bool _Test, class _Ty1, class _Ty2>
struct conditional
{	
	typedef _Ty2 type;
};
//Если _Test == true, то type будет типа _Ty1
template<class _Ty1, class _Ty2>
struct conditional<true, _Ty1, _Ty2>
{	
	typedef _Ty1 type;
};

//Кольцевой буфер
//S: желаемый размер буфера
template<u16 S> struct RingBuffer
{
	private:
	//Тип индексов
	typedef typename conditional<(S>128), u16, u8 >::type T;
	//Округление размера буфера вверх к ближайшему 2^x
	static const u16 S0 = S-1, S1 = S0 | S0 >> 1, S2 = S1 | S1 >> 2, S4 = S2 | S2 >> 4, S8 = S4 | S4 >> 8;
	static const T bufferSize = S8 + 1; //Размер буфера
	volatile T writeIndex = 0, readIndex = 0; //Индексы записи и чтения
	volatile u8 buffer[bufferSize]; //Сам буфер

	public:
	volatile bool isOverflow = false; //Фалг переполнения буфера

	//Функция добавляет байт в конец буфера
	//data: добавляемый байт
	void Push(u8 data)
	{
		if (Size() == bufferSize) //Переполнение
		{
			isOverflow = true;
			return;
		}
		buffer[writeIndex++ & (bufferSize - 1)] = data;
	}
	//Функция возвращает байт из начала буфера и удаляет его из буфера
	u8 Pop()
	{
		if (Empty()) return 0; //Буфер пустой
		return buffer[readIndex++  & (bufferSize - 1)];
	}
	//Функция возвращает байт из начала буфера
	u8 Front() const 
	{
		if (Empty()) return 0;
		return buffer[readIndex & (bufferSize - 1)];
	}
	//Функция возвращает байт из конца буфера
	u8 Back() const 
	{
		if (Empty()) return 0;
		return buffer[writeIndex - 1 & (bufferSize - 1)];
	}
	//Функция очищает буфер
	void Clear()
	{
		readIndex = writeIndex = 0;
	}
	//Функция возвращает занятый размер буфера
	T Size() const
	{
		return writeIndex - readIndex;
	}
	//Функция возвращает true, если буфер пустой
	bool Empty() const
	{
		return writeIndex == readIndex;
	}
};

//Перечисление режимов работы UART (бита четности и количества стоп битов)
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