もしあなたが新しくこれを導入しようとしているなら、 https://docs.vrchat.com/docs/string-loading で実装するべきです

# nS videoParser (VRC_get_text_from_video)
## これは
VRChatで文字列と画像を動画から読み出すためのツールです
[m-hayabusa/send_text_to_vrc](https://github.com/m-hayabusa/send_text_to_vrc) と組み合わせて使います


### サーバー側: 
```javascript
import * as send_text_to_vrc from "send_text_to_vrc";

const files = [];

const table = new send_text_to_vrc.File("list", ["name", "comment"]);
table.push(["エビフライ", "1つ"]);
table.push(["卵", "1パック"]);
files.push(table);

const images = new send_text_to_vrc.Images("img");
images.push("エビフライの画像", "./ebifly.png");
images.push("卵の画像", "./egg.png");
files.push(images);

send_text_to_vrc.publish(files, "./kaimonolist.webm");
// この後、生成された kaimonolist.webm をhttps経由でアクセスできるようにする必要があります
```


### U#側:
1. このパッケージを導入
    * `vpm add repo https://vpm.nekomimi.studio/index.json` を実行して、VRChat CreatorCompanionから`nS videoParser`をプロジェクトに追加  
      (`vpm`コマンドについては[CLI | VRChat Creator Companion: https://vcc.docs.vrchat.com/vpm/cli](https://vcc.docs.vrchat.com/vpm/cli)を参照)
    * https://vpm.nekomimi.studio/ から unitypackage をダウンロードして import
2. Hierarchyを右クリック、 nsVideoParser を追加
3. nsVideoParser についている v2sConfig.Url に、上記 kaimonolist.webm へのURLを入れておく

```csharp
using UnityEngine;
using UdonSharp;
using nekomimiStudio.video2String;

public class kaimonolist : UdonSharpBehaviour
{
    public Video2Str video2Str; // nsVideoParser についている Video2Str をここに割りあてる

    private bool done = false;
    private Parser parser;
    [SerializeField] private RenderTexture[] rTex;
    
    void Start(){
        parser = video2Str.getParser();
    }
    
    void Update()
    {
        if (!done)
        {
            Debug.Log($"progress: {video2Str.getDecodeProgress()}");

            if (parser.isDone())
            {
                for (int i = 0; i < parser.getLength("list"); i++)
                    Debug.Log($"{i}: {parser.getString("list", i, "name")}, {parser.getString("list", i, "comment")}");

                for (int i = 0; i < parser.getLength("img"); i++) {
                    Debug.Log(parser.getString("img", i, "filename"));
                    video2Str.GetTexture(rTex[i], "img", i);
                }

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
