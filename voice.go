package main

import (
	"fmt"
	"github.com/gordonklaus/portaudio"
)

func startVoice() {
	portaudio.Initialize()
	defer portaudio.Terminate()
	h, _ := portaudio.DefaultHostApi()
	if h.DefaultInputDevice == nil && h.DefaultOutputDevice == nil {
		return
	}
	p := portaudio.LowLatencyParameters(h.DefaultInputDevice, h.DefaultOutputDevice)
	p.Input.Channels = 1
	p.Output.Channels = 1
	stream, err := portaudio.OpenStream(p, processAudio)
	if err != nil {
		fmt.Println(err)
		return
	}
	defer stream.Close()
	stream.Start()
	defer stream.Stop()
	<-voiceExit

}
func processAudio(in, out []byte) {
	for i := range out {
		if len(reveiveQueue) > 0 {
			b := <-reveiveQueue
			out[i] = b
		} else {
			out[i] = 0
		}
	}
	for i := range in {
		sendQueue <- in[i]
	}
}
