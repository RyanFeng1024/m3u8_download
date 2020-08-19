import javax.crypto.SecretKey;
import java.io.File;
import java.io.FileOutputStream;
import java.net.MalformedURLException;
import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.stream.Collectors;

public class VideoSpider {

    private static List<String> error_arr = new ArrayList<>();

    /**
     * 下载视频
     * @param DRMKey key
     * @param m3u8Url m3u8在线地址
     * @param savePath 视频保存目录
     * @param saveFileName 保存的文件（夹）名
     */
    public static void run (String DRMKey, String m3u8Url, String savePath, String saveFileName) {
        try {
            String StartDate = new SimpleDateFormat("HH:mm:ss").format(new Date());
            System.out.println(StartDate + " ---- 开始下载 ----");

            // 根据保存的文件名或m3u8文件名，创建本地保存目录
            int index = m3u8Url.lastIndexOf("/");
            String dirName = "".equals(saveFileName) ? m3u8Url.substring(index + 1) : saveFileName;
            String finalSavePath = savePath + dirName + "\\";
            File dir = new File(finalSavePath);
            if (!dir.exists()) {
                dir.mkdir();
            }

            // ----------------  读取线上m3u8文件内容  ----------------
            String m3u8Content = CommonUtils.doGet(m3u8Url);

            // ----------------  读取本地m3u8文件测试 Start ----------------
//            StringBuilder m3u8Content = new StringBuilder();
//            String filePath = "D:\\test2.m3u8";
//            File file = new File(filePath);
//            try {
//                FileReader reader = new FileReader(file);
//                BufferedReader bufferedReader = new BufferedReader(reader);
//                String tempString = null;
//                while ((tempString = bufferedReader.readLine()) != null) {
//                    m3u8Content.append(tempString);
//                    m3u8Content.append("\r\n");
//                }
//                bufferedReader.close();
//            } catch (FileNotFoundException e) {
//                e.printStackTrace();
//            } catch (IOException e) {
//                e.printStackTrace();
//            }
            // ----------------  读取本地m3u8文件测试 End ----------------

            String aesKey = CommonUtils.getAESKey(DRMKey);
            SecretKey skey = CommonUtils.loadKeyAES(aesKey);

            // 根据在线m3u8地址 获取域名，如果操作本地m3u8文件，则直接手动设置 domain
            URL url = new URL(m3u8Url);
            String domain = url.getProtocol() + "://" + url.getAuthority();

            List<String> tsList = CommonUtils.getMatchers("\r\n(.*?.ts)", m3u8Content.toString());
            List<String> ivList = CommonUtils.getMatchers("IV=(.*?)\r\n", m3u8Content.toString());
            if (tsList.size() != ivList.size()) {
                System.out.println("m3u8Content 解析失败");
            } else {
                System.out.println("m3u8Content 解析完成，共有 " + ivList.size() + " 个ts文件");

                List<List<String>> listTuple = CommonUtils.zip(tsList, ivList);
                listTuple.forEach(x -> {
                    String ts = x.get(0).replace("\r\n", "");
                    String iv = x.get(1).replace("\r\n", "");
                    iv = iv.replace("IV=0x", "");
                    iv = iv.substring(0, 16);   //去除前缀，取IV前16位

                    int idx = ts.lastIndexOf("/");
                    String tsFileName = ts.substring(idx + 1);

                    try {
                        String saveFilepath = finalSavePath + tsFileName;
                        File saveFile  = new File(saveFilepath);
                        if (!saveFile.exists()) {
                            System.out.println("开始下载ts: " + domain + ts);
                            byte[] encByte = CommonUtils.doGetByteArr(domain + ts);
                            if (encByte != null) {
                                System.out.println("开始解密, IV -> " + iv);
                                byte[] decByte = null;     //解密视频流
                                try {
                                    decByte = CommonUtils.decryptAESByKey(encByte, skey, iv);
                                } catch (Exception e) {
                                    error_arr.add(tsFileName);
                                    System.out.println("解密ts文件["+tsFileName+"]异常。" + e.getMessage());
                                    e.printStackTrace();
                                }
                                if (decByte != null) {
                                    //保存视频文件
                                    FileOutputStream fos = new FileOutputStream(saveFile);
                                    fos.write(decByte,0,decByte.length);
                                    fos.flush();
                                    fos.close();

                                    Integer ii = listTuple.indexOf(x);
                                    System.out.println(tsFileName + " 下载完成. " + (ii + 1) + "/" + ivList.size());
                                }
                            } else {
                                error_arr.add(tsFileName);
                                System.out.println("doGetByteArr 结果返回null");
                            }
                        } else {
                            System.out.println("文件 " + saveFilepath + " 已存在");
                        }
                    } catch (Exception e) {
                        error_arr.add(tsFileName);
                        e.printStackTrace();
                    }
                });
                System.out.println("所有操作已完成. 保存目录 " + finalSavePath);
                if (error_arr.size() > 0) {
                    List<String> list = error_arr.stream().distinct().collect(Collectors.toList());
                    System.out.println("其中 共有" + list.size() + "个文件下载失败：");
                    list.forEach(x -> {
                        System.out.println(x);
                    });
                } else {
                    // 文件全部下载成功，调用 cmd的 copy /b命令合并  eg: CommonUtils.exeCmd("cmd /c copy /b D:\\cmdtest\\*.ts D:\\cmdtest\\newfile.ts");
                    CommonUtils.exeCmd("cmd /c copy /b " + finalSavePath + "*.ts " + finalSavePath + dirName + ".ts");
                }

                String endDate = new SimpleDateFormat("HH:mm:ss").format(new Date());
                System.out.println(endDate + " ---- 下载完成 ----");
            }
        } catch (MalformedURLException e) {
            e.printStackTrace();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
