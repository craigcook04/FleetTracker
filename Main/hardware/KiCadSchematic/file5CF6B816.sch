EESchema Schematic File Version 4
LIBS:trolleySchematic-cache
EELAYER 29 0
EELAYER END
$Descr A4 11693 8268
encoding utf-8
Sheet 3 3
Title ""
Date ""
Rev ""
Comp ""
Comment1 ""
Comment2 ""
Comment3 ""
Comment4 ""
$EndDescr
$Comp
L Connector:Screw_Terminal_01x04 J1
U 1 1 5CF8953E
P 7350 6150
F 0 "J1" V 7222 6330 50  0000 L CNN
F 1 "Screw_Terminal_01x04" V 7313 6330 50  0000 L CNN
F 2 "" H 7350 6150 50  0001 C CNN
F 3 "~" H 7350 6150 50  0001 C CNN
	1    7350 6150
	0    1    1    0   
$EndComp
$Comp
L Connector:Screw_Terminal_01x04 J2
U 1 1 5CF8817D
P 4650 6200
F 0 "J2" V 4522 6380 50  0000 L CNN
F 1 "Screw_Terminal_01x04" V 4613 6380 50  0000 L CNN
F 2 "" H 4650 6200 50  0001 C CNN
F 3 "~" H 4650 6200 50  0001 C CNN
	1    4650 6200
	0    1    1    0   
$EndComp
$Comp
L bluepill_breakouts:BluePill_STM32F103C U?
U 1 1 5CF806DE
P 5950 3650
AR Path="/5CF806DE" Ref="U?"  Part="1" 
AR Path="/5CF1EF40/5CF806DE" Ref="U?"  Part="1" 
AR Path="/5CF6B817/5CF806DE" Ref="U?"  Part="1" 
F 0 "U?" H 5975 2177 50  0000 C CNN
F 1 "BluePill_STM32F103C" H 5975 2086 50  0000 C CNN
F 2 "BluePill_breakouts:BluePill_STM32F103C" H 6000 2050 50  0001 C CNN
F 3 "" H 5950 2150 50  0001 C CNN
	1    5950 3650
	-1   0    0    1   
$EndComp
Wire Wire Line
	6800 2550 7250 2550
Wire Wire Line
	7250 2550 7250 5950
Wire Wire Line
	6800 2850 7450 2850
Wire Wire Line
	7450 2850 7450 5950
Wire Wire Line
	6800 2950 7350 2950
Wire Wire Line
	7350 2950 7350 5950
Wire Wire Line
	4750 4250 4750 6000
Wire Wire Line
	5050 3750 4550 3750
Wire Wire Line
	4550 3750 4550 6000
Wire Wire Line
	5050 3850 4650 3850
Wire Wire Line
	4650 3850 4650 6000
Wire Wire Line
	5050 4150 4450 4150
Wire Wire Line
	4450 4150 4450 6000
Wire Wire Line
	5050 4150 5950 4150
Wire Wire Line
	5950 4150 5950 4550
Wire Wire Line
	5950 4550 7150 4550
Wire Wire Line
	7150 4550 7150 5950
Connection ~ 5050 4150
$Comp
L Connector:Screw_Terminal_01x04 U1
U 1 1 5CF9A7AB
P 3400 2950
F 0 "U1" H 3318 2525 50  0000 C CNN
F 1 "Ultrasonic Sensor" H 3318 2616 50  0000 C CNN
F 2 "" H 3400 2950 50  0001 C CNN
F 3 "~" H 3400 2950 50  0001 C CNN
	1    3400 2950
	-1   0    0    1   
$EndComp
Wire Wire Line
	3600 2750 4200 2750
Wire Wire Line
	4200 2750 4200 4150
Wire Wire Line
	4200 4150 4450 4150
Connection ~ 4450 4150
Wire Wire Line
	3600 2850 4350 2850
Wire Wire Line
	4350 2850 4350 3550
Wire Wire Line
	4350 3550 5050 3550
Wire Wire Line
	3600 2950 3600 3000
Wire Wire Line
	3600 3000 4050 3000
Wire Wire Line
	4050 3000 4050 3450
Wire Wire Line
	4050 3450 5050 3450
Wire Wire Line
	3600 4250 3600 3050
Wire Wire Line
	3600 4250 4750 4250
Connection ~ 4750 4250
Wire Wire Line
	4750 4250 5050 4250
$EndSCHEMATC
