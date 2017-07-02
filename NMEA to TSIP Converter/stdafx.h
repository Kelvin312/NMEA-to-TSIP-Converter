/*
 * Created: 28.06.2017 14:03:03
 *  Author: Andrey
 */ 


#ifndef STDAFX_H_
#define STDAFX_H_

#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/wdt.h>
#include <avr/power.h>
#include <avr/sleep.h>
#include <avr/eeprom.h>
#include <avr/pgmspace.h> 

#include <string.h>

typedef unsigned char u8;
typedef unsigned short u16;
typedef signed char s8;
typedef signed short s16;



template<u8 bufferSize> struct RingBuffer
{
	volatile u8 writeIndex, readIndex, counter;
	volatile u8 buffer[bufferSize];
	
	void Push(u8 data)
	{
		buffer[writeIndex] = data;
		if(++writeIndex >= bufferSize) writeIndex = 0;
		++counter;
	}
	u8 Pop()
	{
		if(counter == 0) return 0;
		u8 data = buffer[readIndex];
		if(++readIndex >= bufferSize) readIndex = 0;
		--counter;
		return data;
	}
	void Clear()
	{
		readIndex = writeIndex = counter = 0;
	}
	u8 Size()
	{
		return counter;
	}
};



#endif /* STDAFX_H_ */