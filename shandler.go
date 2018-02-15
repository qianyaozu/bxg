package main

import (
	"fmt"
	"net"
	"sync"
)

var sockets sync.Map

func handlerSocketServer() {
	ln, err := net.Listen("tcp", ":5000")
	fmt.Println("Listen")
	if err != nil {
		fmt.Println("listen", err)
	}
	for {
		conn, err := ln.Accept()
		if err != nil {
			fmt.Println(err)
			continue
		}
		sockets.Store(conn.RemoteAddr(), conn)
		go ReadConnection(conn)
		go WriteConnection(conn)
	}
}

func ReadConnection(con net.Conn) {
	var data = make([]byte, 1024)
	for {
		length, err := con.Read(data)
		if err != nil {
			sockets.Delete(con.RemoteAddr())
			fmt.Println(err)
			return
		} else {
			for i := 0; i < length; i++ {
				var b = data[i]
				reveiveQueue <- b
			}
		}
	}
}

func WriteConnection(con net.Conn) {
	for {
		var data = make([]byte, 0)
		for {
			if len(sendQueue) == 0 || len(data) >= 512 {
				break
			}
			data = append(data, <-sendQueue)
		}
		if len(data) > 0 {
			con.Write(data)
		}
	}
}
