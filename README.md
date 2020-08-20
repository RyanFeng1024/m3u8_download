# m3u8_download
#### m3u8加密ts下载，仅针对于加密方式为 QINIU-PROTECTION-10 的视频解密。其他格式未测试.

文章链接 https://www.cnblogs.com/myron1024/p/13532379.html

## 使用方法
下载代码，修改C#或Java代码的这四个参数，改成对应的自己要下载的地址、key和保存路径，然后运行即可
    
    string DRMKey = "11, 22, 33, 44, 55, 66, 77, 88, 99, 00, 111, 111, 111, 111, 111, 111";		//DRMKey
    string m3u8Url = "https://XXXXXXX/123.m3u8"; 	//m3u8在线地址
    string savePath = "D:\\VIDEO\\";		//保存的本地路径
    string saveFileName = "VIDEO_FILE_NAME";    	//保存的文件（夹）名称，如果为空 则使用默认m3u8文件名
   
C#可以创建个控制台应用，代码全部复制粘贴，修改以上参数就可以运行了。Java的也可以直接下载运行，我的JDK版本1.8
本来想做个桌面应用程序的，结果嫌麻烦，费时间就没做了。哪位看官要是有时间可以做个桌面程序方便操作，另外可以加上多线程去下载试试。

> **DRMKey参数说明**：解密视频需要key和IV， 我们可以看到 IV在m3u8文件里有，每一个.ts文件都有一个对应的IV，#EXT-X-KEY:后面的 **IV=xxxxxx** 就是我们需要用到的 IV了， 可是key却没有，需要在网页上找找了，有一点基础的基本都能找到。下面是我找到key的过程，仅供参考：

> 打开控制台后，重新加载页面，发现一个 **qiniu-web-player.js** 在控制台输出了一些配置信息和日志记录，其中的 hls.DRMKey 引起了我的注意
数组长度也是16位，刚好加解密用到的key的长度也是16位,所以这个应该就是AES加解密要用到的key了。不过需要先转换一下。。

![DRMKey](https://github.com/Myron1024/m3u8_download/blob/master/screenshot/DRMKey.png?raw=true)

> 经过一番搜索得知转换步骤为：把数组里每个元素转换成16进制字符串，然后把16进制字符串转为ASCII码，这16个ASCII字符最终拼接出来的结果就是AES的key了。
> 不过**此处DRMKey的参数值只需要配置成数组的字符串格式即可（不包括前后中括号）**

## C#版本运行截图
![csharp](https://github.com/Myron1024/m3u8_download/blob/master/screenshot/c%23.png?raw=true)

## Java版本运行截图
![java](https://github.com/Myron1024/m3u8_download/blob/master/screenshot/java.png?raw=true)


#### 下载完成后，可以用windows自带的cmd执行命令来合并：
`copy /b D:\VIDEO\*.ts D:\VIDEO\newFile.ts` 
#### 或者使用 `合并ts.bat` 批处理文件，放到要合并视频的目录下，运行即可。

也可以使用程序代码在下载完所有ts后直接执行合并命令，详见Java版代码中 `VideoSpider.java` 的131行，C#的当然也可以实现

    // 文件全部下载成功，调用 cmd的 copy /b命令合并  eg: CommonUtils.exeCmd("cmd /c copy /b D:\\cmdtest\\*.ts D:\\cmdtest\\newfile.ts");
    CommonUtils.exeCmd("cmd /c copy /b " + finalSavePath + "*.ts " + finalSavePath + dirName + ".ts");
    

