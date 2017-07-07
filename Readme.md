# NMEA to TSIP Converter

GPS Receivers
* New: CONDOR C2626 / NMEA 0183 (2.8V TXD Pin 5 are 9600 baud 8-None-1)
* Old: Lassen iQ / TSIP (3.3V TXD Pin 1 are 9600 baud 8-odd-1)

Hardware
* Arduino Nano v3.0 (ATmega328P 16МГц 5V)

### TSIP packet structure

    <0x10> <id> <data string bytes> <0x10> <0x03>
    <id> любое значение, кроме <0x10> и <0x03>.
    0x10 в данных надо экранировать добавлением 0x10 ('stuffing') (!!!)
    <0x10> <0x03> означает конец, только если количество <0x10> четно.
    Порядок байт - со старшего
    Single = Float

Automatic Output Packets	
|	   |											|			|
|------|--------------------------------------------|-----------|
| 0x41 | GPS time									| 5 seconds	|
| 0x4A | position (choose packet with I/O options)	| 1 second	|
| 0x56 | velocity (choose packet with I/O options)	| 1 second	|
| 0x46 | health of receiver							| 5 seconds |
| 0x4B | machinecode/status (includes antenna fault detect)	| 5 seconds |
| 0x6D | all-in-view satellite selection, DOPs, Fix Mode	| 1 second |
| 0x82 | DGPS position fix mode (only in DGPS mode)	| 1 second	|

Packets Output at Power-Up
|	   |						|
|------|------------------------|
| 0x45 | software version		|					
| 0x46 | health of receiver	    |
| 0x4B | machinecode/status     |
| 0x4A | position               |
| 0x56 | velocity               |
| 0x41 | GPS time	            |
| 0x82 | DGPS position fix mode |

* UTC = (GPS time) - (GPS UTC offset).
* Неделя №0 началась 6 января 1980 года.
* Счет секунд начинается с «0» Каждое воскресенье в полночь

### NMEA 0183 Message Format

    $GP<MSG><,D1,D2,D3,D4,.......,Dn>*<CS>[CR][LF]
    MSG - индентификатор сообщения
    “,” - разделители полей данных
    Dn - данные
    CS - HЁX контрольная XOR-сумма всех байт в строке между «$» и «*»

|		|			|												|
|-------|-----------|-----------------------------------------------|
| GGA	|	Default | GPS fix data 									|
| GSA	|	Default | GPS DOP and active satellites 				|
| GSV	|	Default | GPS satellites in view						|
| RMC 	|	Default | Recommended minimum specific GPS/Transit data |
| CHN	|	Other	| GPS channel status 							|
| GLL	|	Other	| Geographic position – Latitude/Longitude     	|
| VTG	|	Other	| Track Made Good and Ground Speed				|
| ZDA	|	Other	| Time and date									|
                                                                                                                             |

