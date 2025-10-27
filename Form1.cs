using System.Text.RegularExpressions;
using System;

namespace dewu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(Save);
            thread.IsBackground = true;
            thread.Start();
        }
        ///执行保存
        void Save()
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("请输入内容");
                return;
            }
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("请选择保存位置");
                return;
            }
            Regex regex = new Regex(@"(https://[\s\S]+?)点");
            MatchCollection matches = regex.Matches(textBox1.Text);
            if (matches.Count == 0)
            {
                MessageBox.Show("请输入正确的内容");
                return;
            }
            SimpleThreadPool pool = new SimpleThreadPool(10);
            // 使用线程安全的计数器
            int successCount = 0;
            int failCount = 0;
            int totalCount = matches.Count;
            
            // 获取复选框状态（线程安全）
            bool keepOnlyFirstImage = false;
            bool skipTextContent = false;
            // 使用普通int变量配合Interlocked静态方法实现线程安全计数
            int imgCount = 0;
            Invoke(() =>
            {
                button1.Text = $"正在保存 0/{totalCount}";
                keepOnlyFirstImage = checkBox1.Checked;
                skipTextContent = checkBox2.Checked;
            });
            foreach (Match match in matches)
            {
                string url = match.Groups[1].Value;
                pool.QueueTask(() =>
                {
                    using (HttpRequestClass http = new HttpRequestClass())
                    {
                        http.Open(url);
                        http.SetFollowLocation(true);
                        http.Send();
                        string data = http.GetResponse().Body;
                        Regex regex1 = new Regex(@"(\{""props""[\s\S]+?})<");
                        Match match1 = regex1.Match(data);
                        if (match1.Success)
                        {
                            string json = match1.Groups[1].Value;
                            try
                            {
                                dynamic dynamic = EasyJson.ParseJsonToDynamic(json);
                                foreach (dynamic dynamic1 in dynamic.props.pageProps.metaOGInfo.data)
                                {
                                    string title = dynamic1.content.title;//标题
                                    string videoUrl = dynamic1.content.videoShareUrl;//视频地址
                                    string cover = dynamic1.content.cover.url;//封面
                                    string content = dynamic1.content.content;//文案
                                    // 如果不跳过文案，则保存文案文件
                                    if (!skipTextContent)
                                    {
                                        using (FileStream fileStream = new FileStream(Path.Combine(textBox2.Text, $"文案 - {title}.txt"), FileMode.CreateNew))
                                        {
                                            // 将字符串转换为字节数组再写入
                                            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
                                            fileStream.Write(bytes, 0, bytes.Length);
                                        }
                                    }
                                    // 保存封面图片
                                    if (keepOnlyFirstImage)
                                    {
                                        // 原子操作：如果当前值为0，则增加并返回true，否则返回false
                                        int originalValue = System.Threading.Interlocked.CompareExchange(ref imgCount, 1, 0);
                                        
                                        // 只有第一个进入的线程(originalValue为0)才会保存图片
                                        if (originalValue == 0)
                                        {
                                            http.Open(cover);
                                            http.Send();
                                            using (FileStream fileStream = new FileStream(Path.Combine(textBox2.Text, $"{title}.webp"), FileMode.CreateNew))
                                            {
                                                fileStream.Write(http.GetResponse().rawResult);
                                            }
                                        }
                                    }
                                    string videoName = $"{title}.mp4";
                                    // 检查是否有视频并保存
                                    foreach(dynamic dynamic2 in dynamic1.content.media.list)
                                    {
                                        if (dynamic2.mediaType == "video")
                                        {
                                            http.Open(dynamic2.url);
                                            http.Send();
                                            using (FileStream fileStream = new FileStream(Path.Combine(textBox2.Text, videoName), FileMode.CreateNew))
                                            {
                                                fileStream.Write(http.GetResponse().rawResult);
                                            }
                                            break;
                                        }
                                    }
                                }
                                // 任务成功完成，递增成功计数
                                successCount++;
                            }
                            catch(Exception ex)
                            {
                                failCount++;
                            }
                            finally
                            {
                                Invoke(() =>
                                {
                                    button1.Text = $"正在保存 {successCount}/{totalCount}";
                                });
                            }
                        }
                    }
                });
            }
            pool.Wait();
            
            // 所有任务完成后，恢复按钮文本并显示完成信息
            Invoke(() =>
            {
                button1.Text = "保存(&D)";
                MessageBox.Show($"下载完成！\n成功: {successCount}\n失败: {failCount}\n总计: {totalCount}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // 设置初始目录为当前textBox2中的路径
                if (!string.IsNullOrEmpty(textBox2.Text) && System.IO.Directory.Exists(textBox2.Text))
                {
                    folderBrowserDialog.SelectedPath = textBox2.Text;
                }
                
                // 设置对话框描述
                folderBrowserDialog.Description = "请选择保存位置";
                
                // 显示对话框
                DialogResult result = folderBrowserDialog.ShowDialog();
                
                // 如果用户点击了确定按钮
                if (result == DialogResult.OK)
                {
                    // 更新文本框显示选中的目录
                    textBox2.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }
    }
}
