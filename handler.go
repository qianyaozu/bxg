package main

import (
	"errors"
	"fmt"
	"github.com/gin-gonic/gin"
	"github.com/qianyaozu/bxg/dal"
	. "github.com/qianyaozu/qcommon"
	"net/http"
	"strings"
	"time"
)

//对时
//对版本
//上传文件
//申请打开
//上传关闭日志
//上传倾斜移动报警日志
//心跳
//
//
//心跳，获取返回命令
//1.确认打开
//2.拒绝打开
//3.开启报警
//4.关闭警报
func handleServer() {
	gin.SetMode(gin.ReleaseMode)
	router := gin.Default()
	router.POST("/Upload", Upload)            //POST http://192.168.2.1:12345/Upload?mac=33:01:80:09:ed:72
	router.GET("/CheckTime", CheckTime)       //GET http://192.168.2.1:12345/CheckTime
	router.GET("/CheckVersion", CheckVersion) //GET http://192.168.2.1:12345/CheckVersion?mac=33:01:80:09:ed:72&version=1.0.0
	router.POST("/Close", Close)              //POST http://192.168.2.1:12345/Close?mac=33:01:80:09:ed:72
	router.POST("/Alarm", Alarm)              //POST http://192.168.2.1:12345/Alarm?mac=33:01:80:09:ed:72
	router.POST("/Open", Open)                //POST http://192.168.2.1:12345/Open
	router.GET("/Heart", Heart)               //GET  http://192.168.2.1:12345/Heart?mac=33:01:80:09:ed:72
	fmt.Println("server at " + Port)
	router.Run(":" + Port)
}

//校时
func CheckTime(c *gin.Context) {
	c.JSON(http.StatusOK, ResponseJson(time.Now().Format("2006-01-02 15:04:05"), nil))
}

//region 对版本

func CheckVersion(c *gin.Context) {
	mac := c.Query("mac")
	v := c.Query("version")
	if mac == "" || v == "" {
		c.JSON(http.StatusOK, ResponseJson("", errors.New("mac or version can't be empty")))
		return
	}
	item, err := dal.CabinetInfo_GetOne(mac)
	if err != nil {
		c.JSON(http.StatusOK, ResponseJson("", err))
		return
	}
	if v == item.Version {
		c.JSON(http.StatusOK, ResponseJson("", nil))
	} else {
		c.JSON(http.StatusOK, ResponseJson("/files/"+item.Version+".apk", nil))
	}
}

//endregion

//region 上传文件 post http://192.168.2.1:12345/Upload?mac=12:23:43:ae:ce

func Upload(c *gin.Context) {
	mac := c.Query("mac")
	if mac == "" {
		c.JSON(http.StatusOK, ResponseJson("", errors.New("mac can't be empty")))
		return
	}
	// single file
	file, err := c.FormFile("file")
	if err != nil {
		c.JSON(http.StatusOK, ResponseJson(nil, err))
		return
	}
	var fileName = fmt.Sprintf("%v_%v", strings.Replace(mac, ":", "", -1), time.Now().UnixNano()/1e6)
	if err = c.SaveUploadedFile(file, fileName); err != nil {
		c.JSON(http.StatusOK, ResponseJson(nil, err))
	} else {
		c.JSON(http.StatusOK, ResponseJson(fileName, nil))
	}
}

//endregion

//region 打开
func Open(c *gin.Context) {
	var login Login
	if err := c.Bind(&login); err != nil {
		c.JSON(http.StatusOK, ResponseJson(nil, err))
		return
	}
	//校验参数
	if login.Method == 0 && login.Password == "" {
		c.JSON(http.StatusOK, ResponseJson(nil, errors.New("wrong password")))
		return
	}
	c.JSON(http.StatusOK, ResponseJson("waiting for review", nil))
}

type Login struct {
	Mac         string
	Method      int      //0为密码，1为指纹
	Password    string   //密码
	Photos      []string //照片url
	FingerPrint []string //指纹url
}

//endregion

//region 关闭
func Close(c *gin.Context) {
	mac := c.Query("mac")
	if mac == "" {
		c.JSON(http.StatusOK, ResponseJson("", errors.New("mac can't be empty")))
		return
	}
	c.JSON(http.StatusOK, ResponseJson("", nil))
}

//endregion

//region 上传报警信息
func Alarm(c *gin.Context) {
	mac := c.Query("mac")
	if mac == "" {
		c.JSON(http.StatusOK, ResponseJson("", errors.New("mac can't be empty")))
		return
	}
	c.JSON(http.StatusOK, ResponseJson("", nil))
}

//endregion

//region 心跳
func Heart(c *gin.Context) {
	mac := c.Query("mac")
	if mac == "" {
		c.JSON(http.StatusOK, ResponseJson("", errors.New("mac can't be empty")))
		return
	}
	//0 无命令
	//1.确认打开
	//2.拒绝打开
	//3.开启报警
	//4.关闭警报
	//5.等待审核
	//6.申请语音

	c.JSON(http.StatusOK, ResponseJson(0, nil))
}

//endregion

type Cmder struct {
	CabinetID string
	CmdType   int
}
