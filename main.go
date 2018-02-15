package main

import (
	"github.com/gordonklaus/portaudio"
)

var (
	Port                                     = "12345"
	sampleRate         float64               = 44100
	defaultInputDevice *portaudio.DeviceInfo = nil //默认输入设备
	defaultOutDevice   *portaudio.DeviceInfo = nil //默认输出设备
	reveiveQueue                             = make(chan byte, 10000)
	sendQueue                                = make(chan byte, 10000)
	voiceExit                                = make(chan int, 1)
)

func main() {

	go startVoice()

	handlerSocketServer()
	handleServer()
}
