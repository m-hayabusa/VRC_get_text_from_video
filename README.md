# VRC_get_text_from_video
## これは
VRChatで文字列を動画から読み出すためのツールです
[m-hayabusa/send_text_to_vrc](https://github.com/m-hayabusa/send_text_to_vrc) と組み合わせて使います

* VRCSDK3-WORLD-2021.09.30.16.18_Public
* UdonSharp_v0.20.3

で動作確認

### サーバー側: 
```javascript
import * as send_text_to_vrc from "send_text_to_vrc";

const files = [];
const file = new send_text_to_vrc.File("買い物リスト", ["name", "comment"]);

file.push(["エビフライ", "1つ"]);
file.push(["卵", "1パック"]);

files.push(file);

send_text_to_vrc.publish(files, "./kaimonolist.webm");
// この後、生成された kaimonolist.webm をhttps経由でアクセスできるようにする必要があります
```


### U#側:
* Video2StrCore.prefab を Sceneに追加
* Video2StrCore についている v2sConfig.Url に、上記 kaimonolist.webm へのURLを入れておく

```csharp
using UnityEngine;
using UdonSharp;
using nekomimiStudio.video2String;

public class kaimonolist : UdonSharpBehaviour
{
    public Video2Str video2Str; // Video2StrCore についている Video2Str をここに割りあてる

    private bool done = false;

    void Update()
    {
        if (!done)
        {
            Debug.Log($"progress: {video2Str.getDecodeProgress()}");

            if (video2Str.parser.isParseEnd)
            {
                for (int i = 0; i < video2Str.parser.getLength("買い物リスト"); i++)
                    Debug.Log($"{i}: {video2Str.parser.getString("買い物リスト", i, "name")}, {video2Str.parser.getString("買い物リスト", i, "comment")}");
                done = true;
            }
        }
    }

    public override void Interact()
    {
        video2Str.reload();
    }
}
```