# m3u8_download
#### m3u8加密ts下载，仅针对于加密方式为 QINIU-PROTECTION-10 的视频解密。其他格式未测试.

文章链接 https://www.cnblogs.com/myron1024/p/13532379.html


## C#版本运行截图
![markdown](https://github.com/Myron1024/m3u8_download/blob/master/screenshot/c%23.png?raw=true)

## Java版本运行截图
![markdown](https://github.com/Myron1024/m3u8_download/blob/master/screenshot/java.png?raw=true)

#### C#代码新建个控制台应用，代码复制过去，改一下最上面的四个参数值就可以运行。本来想做个桌面应用程序的，结果嫌麻烦，费时间就没做了。
#### 哪位看官要是有时间可以做个桌面程序方便操作，另外可以加上多线程去下载会快一些。下载完成后，可以用windows自带的cmd执行命令：
`copy /b D:\VIDEO\*.ts D:\VIDEO\newFile.ts` 
#### 来合并ts文件
