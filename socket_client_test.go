package main

import (
	"fmt"
	"math/rand"
	"net"
	"testing"
)

func TestSocket(t *testing.T) {
	con, err := net.Dial("tcp", "127.0.0.1:5000")
	if err != nil {
		fmt.Println("Dial", err)
		return
	}
	defer con.Close()
	var data = make([]byte, 512)
	index := 0
	for {
		index++
		for i := 0; i < 512; i++ {
			data[i] = byte(rand.Intn(255))
		}
		fmt.Println(index, " ")
		fmt.Println(con.Write(data))
	}
}
