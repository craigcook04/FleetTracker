#ifndef SERIAL_USB
    #define SERIAL_USB
#endif

#include <Arduino.h>
#include <HardwareSerial.h>
extern HardwareSerial Serial3;

//#define USB_SERIAL_ENBALED

#ifdef USB_SERIAL_ENBALED
    #define USB_PRINT(...) Serial.print(__VA_ARGS__)
    #define USB_PRINTLN(...) Serial.println(__VA_ARGS__)
    #define USB_INIT(speed) Serial.begin(speed);
#else
    #define USB_PRINT(...)
    #define USB_PRINTLN(...)
    #define USB_INIT(speed)
#endif

#define PIN_TRIGGER_FRONT PB6
#define PIN_ECHO_FRONT PB7
#define PIN_TRIGGER_BACK PB4
#define PIN_ECHO_BACK PB3

#define LIMIT_LOWER 80
#define LIMIT_UPPER 90
#define CYCLE_TIME 50
#define COUNT_MAX 30

#define IN_RANGE(distance) (distance >= LIMIT_LOWER && distance <= LIMIT_UPPER)

#define SIG_REQUEST 1

enum class UState { low, rising, high, falling };
struct USensor {
    UState curState = UState::low;
    float disCur = 100;
    uint32_t fallTime = 0;
    uint32_t count = 0;
    uint32_t countLst = 0;

    const int pinTrigger;
    const int pinEcho;

    USensor(int pinTrigger, int pinEcho) : pinTrigger(pinTrigger), pinEcho(pinEcho) {
        pinMode(pinTrigger, OUTPUT);
        pinMode(pinEcho, INPUT);
    }
};

float ultrasonicReadCM(int pinTrigger, int pinEcho) {
    digitalWrite(pinTrigger, LOW);
    delayMicroseconds(2);
    digitalWrite(pinTrigger, HIGH);
    delayMicroseconds(10);
    digitalWrite(pinTrigger, LOW);

    uint32_t duration = pulseIn(pinEcho, HIGH, CYCLE_TIME * 1000);
    return (duration * 0.0343) / 2;
}

void handleUltrasonic(USensor *sensor) {
    sensor->countLst = sensor->count;

    uint32_t curTime = millis();
    sensor->disCur = ultrasonicReadCM(sensor->pinTrigger, sensor->pinEcho);

    switch (sensor->curState) {
        case UState::low: {
            if (!IN_RANGE(sensor->disCur)) {
                sensor->curState = UState::high;
                
                sensor->count++;
            }

            break;
        }

        case UState::high: {
            if (IN_RANGE(sensor->disCur)) {
                sensor->curState = UState::falling;
                sensor->fallTime = curTime;
            }

            break;
        }

        case UState::falling: {
            if (curTime >= sensor->fallTime + 400) {
                sensor->curState = UState::low;
            }
            else if (!IN_RANGE(sensor->disCur)) {
                sensor->curState = UState::high;
            }

            break;
        }
    }

    int32_t sleepTime = CYCLE_TIME - (millis() - curTime);
    if (sleepTime > 0)
        delay(sleepTime);
}

USensor frontSensor(PIN_TRIGGER_FRONT, PIN_ECHO_FRONT);
USensor backSensor(PIN_TRIGGER_BACK, PIN_ECHO_BACK);
uint32_t count = 0;
uint32_t entered = 0;
uint32_t exited = 0;
bool front = false, back = false;
uint32_t triggerTime = 0;
uint32_t sendTime = 0;

void setup() {
    USB_INIT(115200);
    Serial3.begin(2400);
}

void loop() {
    handleUltrasonic(&frontSensor);

    uint32_t curTime = millis();
    if (back && curTime > triggerTime + 750) {
        back = false;
    }

    if (frontSensor.count > frontSensor.countLst) {
        if (back) {
            back = false;
            if (count > 0) count--;
            exited++;
            sendTime = curTime + 10000;
            USB_PRINTLN(count);
        }
        else {
            front = true;
            triggerTime = millis();
        }
    }

    
    handleUltrasonic(&backSensor);

    if (front && millis() > triggerTime + 750) {
        front = false;
    }

    if (backSensor.count > backSensor.countLst) {
        if (front) {
            front = false;
            if (count < COUNT_MAX) count++;
            entered++;
            sendTime = curTime + 10000;
            USB_PRINTLN(count);
        }
        else {
            back = true;
            triggerTime = millis();
        }
    }

    if (Serial3.available()) {
        switch (Serial3.read()) {
            case SIG_REQUEST:
                uint32_t countCopy = count;
                for (int i = 0; i < 4; i++) {
                    Serial3.write(countCopy & 255);
                    countCopy >>= 8;
                }
                uint32_t enteredCopy = entered;
                for (int i = 0; i < 4; i++) {
                    Serial3.write(enteredCopy & 255);
                    enteredCopy >>= 8;
                }
                uint32_t exitedCopy = exited;
                for (int i = 0; i < 4; i++) {
                    Serial3.write(exitedCopy & 255);
                    exitedCopy >>= 8;
                }
                entered = 0;
                exited = 0;
                break;
        }
    }
}