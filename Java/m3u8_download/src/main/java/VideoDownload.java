public class VideoDownload {
    public static void main(String[] args) {
        String DRMKey = "74, 75, 54, 109, 84, 76, 100, 116, 71, 99, 110, 56, 109, 98, 113, 103";
        String m3u8Url = "https://XXXXXXX/123.m3u8";	//m3u8在线地址
        String savePath = "D:\\VIDEO\\";				//保存的本地路径
        String saveFileName = "VIDEO_FILE_NAME";    	//保存的文件（夹）名称，如果为空 则使用默认m3u8文件名

        VideoSpider.run(DRMKey, m3u8Url, savePath, saveFileName);
    }
}
