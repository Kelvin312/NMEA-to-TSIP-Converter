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
#include <math.h>

typedef unsigned char u8;
typedef signed char s8;
typedef unsigned short u16;
typedef signed short s16;
typedef unsigned long u32;
typedef signed long s32;

#define LED_PIN _BV(5)
#define LED_PORT PORTB


template<u8 bufferSize> struct RingBuffer
{
	volatile u8 writeIndex, readIndex, counter, isOverflow;
	volatile u8 buffer[bufferSize];
	
	volatile void Push(volatile u8 data)
	{
		buffer[writeIndex] = data;
		if(++writeIndex >= bufferSize) writeIndex = 0;
		if(++counter == bufferSize) isOverflow = 1;
	}
	volatile u8 Pop()
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
	volatile u8 Size()
	{
		return counter;
	}
};



#endif /* STDAFX_H_ */