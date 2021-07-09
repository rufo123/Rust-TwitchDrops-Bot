using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace RustLoot
{




    class Program
    {

        private static bool aStopProgram;

        private static int aTimesRefreshed;



        static void Main(string[] args)
        {

            Dictionary<string, bool> tmpUserDefinedWatched = new Dictionary<string, bool>()
            {
            };

            string tmpFirstLine = "";
            string tmpStreamerName = "";
            bool tmpStreamerWatched;

            try
            {
                StreamWriter tmpWriter = File.AppendText("streamerCache.cache");
                tmpWriter.Close();
                StreamReader tmpStreamReader = new StreamReader("streamerCache.cache");
                tmpFirstLine = tmpStreamReader.ReadLine();
                tmpStreamReader.DiscardBufferedData();
                tmpStreamReader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                while (!tmpStreamReader.EndOfStream)
                {
                    tmpStreamerName = tmpStreamReader.ReadLine();
                    tmpStreamerWatched = Convert.ToBoolean(tmpStreamReader.ReadLine());
                    if (tmpStreamerName != null)
                    {
                        tmpUserDefinedWatched.Add(tmpStreamerName, tmpStreamerWatched);
                    }
                }
                tmpStreamReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }


            Dictionary<string, bool> tmpAlreadyWatched = new Dictionary<string, bool>();



            bool checkedUserDefined = false;

            bool firstLoop = false;

            

            string tmpUrl = "https://twitch.facepunch.com/";
            HtmlWeb tmpWeb = new HtmlWeb()
            {
                UsingCache = false
            };

            aStopProgram = false;

            aTimesRefreshed = 0;

            while (!aStopProgram)
            {

                HtmlDocument tmpDoc = tmpWeb.Load(tmpUrl);

                HtmlNode tmpStreamerDrops =
                    tmpDoc.DocumentNode.SelectSingleNode("//section[contains(@class, 'section streamer-drops')]");
                IEnumerable<HtmlNode> tmpStreamersDropsCollection =
                    tmpStreamerDrops.Descendants("a").Where(res => res.HasClass("drop"));

                List<HtmlNode> streamersDropsCollection = tmpStreamersDropsCollection.ToList();

                int openedStreamer = -1;



                for (int i = 0; i < streamersDropsCollection.Count(); i++)
                {

                    // HtmlNode firstOrDefault = tmpStreamerDrops.Descendants("div").FirstOrDefault(res => res.GetAttributeValue("class", "").Contains("streamer-item"));
                    HtmlNode tmpStreamerItemEnumerable = streamersDropsCollection[i].Descendants("div")
                        .FirstOrDefault(res => res.HasClass("streamer-item"));

                    if (tmpStreamerItemEnumerable != null)
                    {

                        firstLoop = false;

                        // Filling up Dictionary with all Streamers
                        if (tmpAlreadyWatched.Count < streamersDropsCollection.Count())
                        {
                            if (!tmpAlreadyWatched.ContainsKey(tmpStreamerItemEnumerable.ChildNodes["span"].InnerHtml))
                            {
                                tmpAlreadyWatched.Add(tmpStreamerItemEnumerable.ChildNodes["span"].InnerHtml, false);
                                Console.WriteLine("Found streamer: ");
                                Console.WriteLine(tmpStreamerItemEnumerable.ChildNodes["span"].InnerHtml);
                                firstLoop = true;

                            }

                        }

                        // Firstly we go through already watched streamers

                        if (tmpAlreadyWatched.TryGetValue(tmpStreamerItemEnumerable.ChildNodes["span"].InnerHtml,
                            out var isCurrentStreamerAlreadyWatched))
                        {

                            if (checkedUserDefined && isCurrentStreamerAlreadyWatched == false &&
                                tmpStreamerItemEnumerable.ChildNodes["div"].InnerHtml == "Live" && openedStreamer < 0)
                            {
                                System.Diagnostics.Process.Start(streamersDropsCollection[i]
                                    .GetAttributeValue("href", String.Empty));
                                tmpAlreadyWatched[tmpStreamerItemEnumerable.ChildNodes["span"].InnerHtml] = true;
                                openedStreamer = i;
                            }
                        }

                        if (checkedUserDefined)
                        {

                            if (tmpAlreadyWatched.ToList()[i].Value)
                            {
                                Console.ResetColor();
                            }
                            else if (!tmpAlreadyWatched.ToList()[i].Value &&
                                     tmpStreamerItemEnumerable.ChildNodes["div"].InnerHtml == "Live")
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }

                            Console.WriteLine(tmpAlreadyWatched.ToList()[i].Key + " " +
                                              tmpAlreadyWatched.ToList()[i].Value);
                            Console.WriteLine(tmpStreamerItemEnumerable.ChildNodes["div"].InnerHtml);

                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine("Refreshed " + aTimesRefreshed + " Times.");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine();
                Console.WriteLine("(Refreshing Every 10 Seconds)");
                Console.ResetColor();
                

                if (!checkedUserDefined)
                {
                    for (int i = 0; i < tmpAlreadyWatched.Count; i++)
                    {
                        if (tmpUserDefinedWatched.ContainsKey(tmpAlreadyWatched.ToList()[i].Key))
                        {
                            tmpAlreadyWatched[tmpAlreadyWatched.ToList()[i].Key] = true;
                        }
                    }

                    checkedUserDefined = true;
                }



                if (openedStreamer >= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Opening: " + tmpAlreadyWatched.ToList()[openedStreamer]);
                    openedStreamer = -1;
                    Thread.Sleep(7200000);

                    for (int i = 0; i < tmpAlreadyWatched.Count; i++)
                    {
                        if (tmpAlreadyWatched.ToList()[i].Value == false)
                        {
                            aStopProgram = false;
                            break;
                        }

                        // If all Streamers Watched
                        aStopProgram = true;
                        // All streamers drops collected
                        Console.WriteLine("All Streamers Drops Collected");
                        Thread.Sleep(5000);
                        aStopProgram = true;
                        return;



                    }
                }
                else if (!firstLoop)
                {
                    Thread.Sleep(10000);

                }


                Console.Clear();
                Console.ResetColor();
                aTimesRefreshed += 1;

            }

        }

    }



}


