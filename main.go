package main

import (
	"fmt"
	"github.com/gordonklaus/portaudio"
)

var (
	Port = "12345"
)

func main() {
	portaudio.Initialize()
	fmt.Println(portaudio.DefaultOutputDevice())
	defer portaudio.Terminate()
	handleServer()
}
