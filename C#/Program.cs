using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace VideoDownload
{
    class Program
    {
        private static List<string> error_arr = new List<string>();

        static void Main(string[] args)
        {
            string DRMKey = "11, 22, 33, 44, 55, 66, 77, 88, 99, 00, 111, 111, 111, 111, 111, 111";		//DRMKey
            string m3u8Url = "https://XXXXXXX/123.m3u8"; 	//m3u8在线地址
            string savePath = "D:\\VIDEO\\";				//保存的本地路径
            string saveFileName = "VIDEO_FILE_NAME";    	//保存的文件（夹）名称，如果为空 则使用默认m3u8文件名

            try
            {
                // 创建本地保存目录
                int index = m3u8Url.LastIndexOf("/");
                string dirName = string.IsNullOrEmpty(saveFileName) ? m3u8Url.Substring(index + 1) : saveFileName;
                string finalSavePath = savePath + dirName + "\\";
                if (!Directory.Exists(finalSavePath))
                {
                    Directory.CreateDirectory(finalSavePath);
                }

                // 读取m3u8文件内容
                string m3u8Content = HttpGet(m3u8Url);
                //string m3u8Content = File.ReadAllText("D:/test.m3u8");

                string aesKey = getAESKey(DRMKey);
                //Console.WriteLine("aesKey:" + aesKey);

                Uri uri = new Uri(m3u8Url);
                string domain = uri.Scheme + "://" + uri.Authority;
                //Console.WriteLine("m3u8域名为：" + domain);

                List<string> tsList = Regex.Matches(m3u8Content, @"\n(.*?.ts)").Select(m => m.Value).ToList();
                List<string> ivList = Regex.Matches(m3u8Content, @"IV=(.*?)\n").Select(m => m.Value).ToList();
                if (tsList.Count != ivList.Count || tsList.Count == 0)
                {
                    Console.WriteLine("m3u8Content 解析失败");
                }
                else
                {
                    Console.WriteLine("m3u8Content 解析完成，共有 " + ivList.Count + " 个ts文件");

                    for (int i = 0; i < tsList.Count; i++)
                    {
                        string ts = tsList[i].Replace("\n", "");
                        string iv = ivList[i].Replace("\n", "");
                        iv = iv.Replace("IV=0x", "");
                        iv = iv.Substring(0, 16);   //去除前缀，取IV前16位

                        int idx = ts.LastIndexOf("/");
                        string tsFileName = ts.Substring(idx + 1);

                        try
                        {
                            string saveFilepath = finalSavePath + tsFileName;
                            if (!File.Exists(saveFilepath))
                            {
                                Console.WriteLine("开始下载ts: " + domain + ts);
                                byte[] encByte = HttpGetByte(domain + ts);
                                if (encByte != null)
                                {
                                    Console.WriteLine("开始解密, IV -> " + iv);
                                    byte[] decByte = null;
                                    try
                                    {
                                        decByte = AESDecrypt(encByte, aesKey, iv);
                                    }
                                    catch (Exception e1)
                                    {
                                        error_arr.Add(tsFileName);
                                        Console.WriteLine("解密ts文件异常。" + e1.Message);
                                    }
                                    if (decByte != null)
                                    {
                                        //保存视频文件
                                        File.WriteAllBytes(saveFilepath, decByte);
                                        Console.WriteLine(tsFileName + " 下载完成");
                                    }
                                }
                                else
                                {
                                    error_arr.Add(tsFileName);
                                    Console.WriteLine("HttpGetByte 结果返回null");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"文件 {saveFilepath} 已存在");
                            }
                        }
                        catch (Exception ee)
                        {
                            error_arr.Add(tsFileName);
                            Console.WriteLine("发生异常。" + ee);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生异常。" + ex);
            }

            Console.WriteLine("所有操作已完成. 保存目录 " + savePath);
            if (error_arr.Count > 0)
            {
                List<string> list = error_arr.Distinct().ToList();
                Console.WriteLine($"其中 共有{error_arr.Count}个文件下载失败：");
                list.ForEach(x =>
                {
                    Console.WriteLine(x);
                });
            }
            Console.ReadKey();
        }


        private static string getAESKey(string key)
        {
            string[] arr = key.Split(",");
            string aesKey = "";
            for (int i = 0; i < arr.Length; i++)
            {
                string tmp = int.Parse(arr[i].Trim()).ToString("X");     //10进制转16进制
                tmp = HexStringToASCII(tmp);
                aesKey += tmp;
            }
            return aesKey;
        }

        /// <summary>
        /// 十六进制字符串转换为ASCII
        /// </summary>
        /// <param name="hexstring">一条十六进制字符串</param>
        /// <returns>返回一条ASCII码</returns>
        public static string HexStringToASCII(string hexstring)
        {
            byte[] bt = HexStringToBinary(hexstring);
            string lin = "";
            for (int i = 0; i < bt.Length; i++)
            {
                lin = lin + bt[i] + " ";
            }
            string[] ss = lin.Trim().Split(new char[] { ' ' });
            char[] c = new char[ss.Length];
            int a;
            for (int i = 0; i < c.Length; i++)
            {
                a = Convert.ToInt32(ss[i]);
                c[i] = Convert.ToChar(a);
            }
            string b = new string(c);
            return b;
        }

        /// <summary>
        /// 16进制字符串转换为二进制数组
        /// </summary>
        /// <param name="hexstring">用空格切割字符串</param>
        /// <returns>返回一个二进制字符串</returns>
        public static byte[] HexStringToBinary(string hexstring)
        {
            string[] tmpary = hexstring.Trim().Split(' ');
            byte[] buff = new byte[tmpary.Length];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = Convert.ToByte(tmpary[i], 16);
            }
            return buff;
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] cipherText, string Key, string IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            byte[] res = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Encoding.ASCII.GetBytes(Key);
                aesAlg.IV = Encoding.ASCII.GetBytes(IV);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] tmp = new byte[cipherText.Length + 32];
                        int len = csDecrypt.Read(tmp, 0, cipherText.Length + 32);
                        byte[] ret = new byte[len];
                        Array.Copy(tmp, 0, ret, 0, len);
                        res = ret;
                    }
                }
            }
            return res;
        }


        public static string HttpGet(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 20000;
                var response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.Write("HttpGet 异常，" + ex.Message);
                Console.Write(ex);
                return "";
            }
        }

        public static byte[] HttpGetByte(string url)
        {
            try
            {
                byte[] arraryByte = null;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 20000;
                request.Method = "GET";
                using (WebResponse wr = request.GetResponse())
                {
                    int length = (int)wr.ContentLength;
                    using (StreamReader reader = new StreamReader(wr.GetResponseStream(), Encoding.UTF8))
                    {
                        HttpWebResponse response = wr as HttpWebResponse;
                        Stream stream = response.GetResponseStream();
                        //读取到内存
                        MemoryStream stmMemory = new MemoryStream();
                        byte[] buffer1 = new byte[length];
                        int i;
                        //将字节逐个放入到Byte 中
                        while ((i = stream.Read(buffer1, 0, buffer1.Length)) > 0)
                        {
                            stmMemory.Write(buffer1, 0, i);
                        }
                        arraryByte = stmMemory.ToArray();
                        stmMemory.Close();
                    }
                }
                return arraryByte;
            }
            catch (Exception ex)
            {
                Console.Write("HttpGetByte 异常，" + ex.Message);
                Console.Write(ex);
                return null;
            }
        }
    }
}
