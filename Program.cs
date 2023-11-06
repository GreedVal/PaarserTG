    using System;
    using System.IO;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using TL;
    using WTelegram;


class Program
{
            static long chatIdToMonitor = 6097132360; // id группы которую надо парсить 
            static Client client;

            static async Task Main(string[] args)
            {
                client = new WTelegram.Client();
                var my = await client.LoginUserIfNeeded();
                Console.WriteLine($"We are logged-in as {my.username ?? my.first_name + " " + my.last_name} (id {my.id})");

                client.OnUpdate += onUpdate;
                Console.ReadKey();
            }

            private static async Task onUpdate(UpdatesBase updates)
            {
                string pathMedia = Path.Combine(Directory.GetCurrentDirectory(), "media"); // Путь сохранения файла медиа
                string pathText = "text.txt"; // Путь сохранения файла текста в виде json


                foreach (var update in updates.UpdateList)
                    switch (update)
                    {
                        case UpdateNewMessage mes:
                            if (mes.message.Peer.ID == chatIdToMonitor) 
                        {
                            string nameFile = "";
                            Message message = (Message) mes.message;
                            if (message.media is MessageMediaDocument { document: Document document })
                            {
                                var filename = document.Filename; 
                                filename ??= $"{document.id}.{document.mime_type[(document.mime_type.IndexOf('/') + 1)..]}";
                                filename = Path.Combine(pathMedia, filename);
                                Console.WriteLine("Downloading " + filename);
                                using var fileStream = File.Create(filename);
                                await client.DownloadFileAsync(document, fileStream);
                                Console.WriteLine("Download finished");
                                nameFile = filename;
                            }
                            else if (message.media is MessageMediaPhoto { photo: Photo photo })
                            {
                                var filename = $"{photo.id}.jpg";
                                filename = Path.Combine(pathMedia, filename);
                                Console.WriteLine("Downloading " + filename);
                                using var fileStream = File.Create(filename);
                                var type = await client.DownloadFileAsync(photo, fileStream);
                                fileStream.Close(); // necessary for the renaming
                                Console.WriteLine("Download finished");
                                if (type is not Storage_FileType.unknown and not Storage_FileType.partial)
                                File.Move(filename, $"{photo.id}.{type}", true); // rename extension
                                nameFile = filename;
                            }

                            List<string> messages = new List<string>();
                            
                            messages.Add("media:" + nameFile);
                            messages.Add("text:" + message.message);
                            messages.Add("date:" + message.date.ToString());
                            messages.Add("|");
                            File.AppendAllLines(pathText, messages);

                        }; 
                        break;
                    }
            }
    }

















