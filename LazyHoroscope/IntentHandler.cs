using LazyHoroscope.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LazyHoroscope
{
    public static class IntentHandler
    {
        public static IntentResult Handle(string intentName)
        {
            var result = new IntentResult();

            switch (intentName)
            {
                case "ZodiacIntent":
                    // 占い結果を返す。今回は適当占いなのでランダムにどちらか
                    // (本来はここでスロットの星座を受け取ってそれに基づいた処理を行う)
                    if (new System.Random().Next() % 2 == 0)
                    {
                        result.AddMessage("今日は絶好調！とてもよい1日になりますよ！");
                    }
                    else
                    {
                        result.AddMessage("今日はあまりついてないかも。転ばないように気を付けてくださいね。");
                    }
                    result.ShouldEndSession = true;
                    break;

                case "AMAZON.HelpIntent":
                case "Clova.GuideIntent":
                    // 使い方
                    result.AddMessage("あなたの星座を教えてください。占ってあげます。");
                    result.ShouldEndSession = false;
                    break;

                default:
                    // その他のインテントの場合
                    result.AddMessage("よくわかりませんでしたが、まあまあだと思います。");
                    result.ShouldEndSession = false;
                    break;
            }

            return result;
        }
    }
}
