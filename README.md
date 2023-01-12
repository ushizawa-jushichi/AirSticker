<p align="center">
<img src="Documentation/header.png" alt="CyDecal(仮)">
</p>

# CyDecal(仮)

## Section 1 Summary
CyDecal is a decal process that complements the disadvantages of URP decals and operates very lightly.<br/>
Also, URP decals can only be used with Unity2021 or later, while CyDecal supports operation from Unity2020.<br/>
For technical documents for engineers, please refer to the following.<br/>

**Technical Documents** ([英語](README_DEVELOPERS.md))

## Section 2 Feature
CyDecal implements decal processing using the typical mesh generation method used in many games.<br/>

Mesh-generated decals realize decal expression by generating a mesh at runtime with a shape that matches the model to which the decal is to be applied, and then applying a texture to it.<br/>

On the other hand, the decal process implemented in Unity implements projected DBuffer decals and screen space decals.<br/>

Both mesh generation and projection decals have advantages/disadvantages.<br/>
The mesh generation and projection methods can also be used together to compensate for many of the disadvantages (see 2.1 and 2.2 for details).<br/>

### 2.1 Advantages and Disadvantages of URP Decal and CyDecal
The advantages/disadvantages of URP decals and CyDecal are as follows.

- **URP Decal**
  - **Advantages**
    - Fast-applied decal.
    - Z-fighting doesn't happen.
  - **Demerit**
    - Difficult to support full skin animation. ( Can be complemented with CyDecal. )
    - Pixel shaders are overloaded.( Can be complemented with CyDecal. )
    -  Custom shaders cannot be used as is.( Can be complemented with CyDecal. )
- **CyDecal**
  -  **Advantages**
     - Lightweight processing.( However, decal mesh generation is laggy )
     - Full skin animation is possible.
     - Custom shaders can be used without modification.
  - **デメリット**
    - The process of applying decals takes time. ( Can be complemented with URP decals. )
    - Z-fighting happen.

Thus, the two decals can be used together to complement many of the disadvantages.<br/>

### 2.2 URPデカールとCyDecalの併用
前節で見たように、二つのデカールの処理を併用することで、多くのデメリットを補完できます。<br/><br/>
ここでは併用の仕方として、次のモデルケースを提示します。

|手法|使用ケース|
|---|---|
|URPデカール| ・ オブジェクト座標系でデカールが移動する<br/> ・ CyDecalによるメッシュ生成が終わるまでの時間稼ぎ|
|CyDecal|オブジェクト座標系でデカールが移動しない|

下記の動画はこのモデルケースで実装しているプレイデモになります。

<br/>
<p align="center">
<img width="80%" src="Documentation/fig-001.gif" alt="URPデカールとCyDecalの使い分け"><br>
<font color="grey">URPデカールとCyDecalの使い分け</font>
</p>

この動画ではレシーバーオブジェクト上でデカールが移動する場合と、メッシュ生成完了までの時間稼ぎの用途でURPデカールを使っています。<br/>
レシーバーオブジェクト上での位置が確定して、メッシュ生成が終わると、以降はCyDecalによるデカールを表示しています。<br/>

メッシュ生成完了後からはCyDecalを使用することによって、モバイルゲームにおいて、致命的となるランタイムパフォーマンスの悪化という問題を大きく改善できます(詳細は2.3を参照)。<br/>

### 2.3 URPデカールとCyDecalの描画パフォーマンス
メッシュ生成方式はメッシュ生成に時間がかかりますが、描画パフォーマンスは単なるメッシュ描画と同じです。<br/>
一方、URPデカールはメッシュ生成を行う必要はありませんが、デカール表示のために複雑な描画処理が実行されます。<br/><br/>
そのため、毎フレームの描画パフォーマンスではメッシュ生成方式の方が有利になります。<br/><br/>
次の図はURPデカールとCyDecalの描画パフォーマンスの計測結果です。<br/>
全てのケースで、CyDecalが優位な結果を出しており、最も顕著に差がでたケースでは19ミリ秒ものパフォーマンスの向上が確認されています。
<p align="center">
<img width="80%" src="Documentation/fig-002.png" alt="パフォーマンス計測結果"><br>
<font color="grey">パフォーマンス計測結果</font>
</p>


## Section 3 使用方法
CyDecalはAssets/CyDecalフォルダーを自身のプロジェクトに取り込むことで利用できます。<br/>
その中でも次の２つのクラスが重要になってきます。
1. CyDecalSystemクラス
2. CyDecalProjectorクラス

### 3.1 CyDecalSystemクラス
CyDecalを利用するためには、必ず、このコンポーネントが貼られたゲームオブジェクトを一つ設置する必要があります。

<p align="center">
<img width="80%" src="Documentation/fig-013.png" alt="CyDecalSystem"><br>
<font color="grey">CyDecalSystem</font>
</p>

### 3.2 CyDecalProjectorクラス
デカールを投影するためのコンポーネントです。デカールプロジェクタとして設置するゲームオブジェクトにこのコンポーネントを追加してください。

<p align="center">
<img width="50%" src="Documentation/fig-004.png" alt="CyDecalProjectorのインスペクタ"><br>
<font color="grey">CyDecalProjectorのインスペクタ</font>
</p>

CyDecalProjectorコンポーネントには5つのパラメータを設定することができます。
|パラメータ名|説明|
|---|---|
|Width|Projector バウンディングボックスの幅です。URPのデカールプロジェクタの仕様に準拠しています。<br/>詳細は[URPデカールのマニュアル](https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-feature-decal.html)を参照してください。 |
|Height|Projector バウンディングボックスの高さです。URPのデカールプロジェクタの仕様に準拠しています。<br/>詳細は[URPデカールのマニュアル](https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-feature-decal.html)を参照してください。|
|Depth|Projector バウンディングボックスの深度です。URPのデカールプロジェクタの仕様に準拠しています。<br/>詳細は[URPデカールのマニュアル](https://docs.unity3d.com/ja/Packages/com.unity.render-pipelines.universal@14.0/manual/renderer-feature-decal.html)を参照してください。|
|Receiver Object| デカールテクスチャの貼り付け対象となるオブジェクト。<br/>CyDecalProjectorは設定されているレシーバーオブジェクトの子供(自身を含む)に貼られている全てのレンダラーを貼り付け対象とします。<br/><br/>そのため、レシーバーオブジェクトはMeshRendererやSkinMeshRendererなどのコンポーネントが貼られているオブジェクトを直接指定もできますし、レンダラーが貼られているオブジェクトを子供に含んでいるオブジェクトの指定でも構いません。<br/>処理するレンダラーの数が多いほど、デカールメッシュ生成の時間がかかるようになるため、貼り付ける範囲を限定できるときは、レンダラーが貼り付けられているオブジェクトの直接指定が推奨されます。<br/><br/>例えば、キャラエディットなどでキャラクターの顔にステッカーを貼り付けたい場合、キャラのルートオブジェクトを指定するよりも顔のレンダラーが貼られているオブジェクトを指定するとメッシュ生成の時間を短縮できます。|
|Decal Material| デカールマテリアル。<br/>URPのデカールマテリアルとは意味あいが違うので注意してください。<br/>URPデカールではShader Graphs/Decalシェーダーが割り当てられたマテリアルしか使えません。<br/>しかし、CyDecalでは通常のマテリアルが使えます。<br/>つまり、ビルトインのLitシェーダー、Unlitシェーダー、そして、ユーザーカスタムの独自シェーダーも利用できます。|
|Launch On Awake|このチェックボックスにチェックが入っていると、インスタンスの生成と同時にデカールの投影処理が開始されます。|
|On Finished Launch|デカールの投影終了時に呼び出されるコールバックを指定できます。|

次の動画はCyDecalProjectorをシーンに設置して使用する方法です。
<p align="center">
<img width="80%" src="Documentation/fig-012.gif" alt="CyDecalProjectorの使用方法"><br>
<font color="grey">CyDecalProjectorの使用方法</font>
</p>

> **Note**<br/>
> 現在、CyDecalProjectorは投影範囲の可視化に対応していないため、シーンビューで配置する場合はURPプロジェクターと併用すると、視覚的に分かりやすくなります。

### 3.3 ランタイムでのCyDecalProjectorの生成
デカールのランタイムでの使用例として、FPSなどの弾痕を背景に貼り付ける処理があります。このような処理をCyDecalで行うためには、背景と銃弾との衝突判定を行い、衝突点の情報を元にCyDecalProjectorコンポーネントを生成して、デカールメッシュを構築することで実現できます。<br/><br/>
CyDecalProjectorコンポーネントはCyDecalProjector.CreateAndLaunch()メソッドを呼び出すことで生成できます。</br>
CreateAndLaunch()メソッドのlaunchAwake引数にtrueを指定すると、コンポーネントの生成と同時にデカールメッシュの構築処理が開始されます。<br/><br/>
デカールメッシュの構築処理は時間のかかる処理になっているため、数フレームにわたって処理が実行されます。そのため、デカールメッシュの構築処理の終了を監視したい場合は、CyDecalProjectorのNowStateプロパティを監視するか、メッシュ生成処理の終了時に呼び出しされる、onFinishedLaunchコールバックを利用する必要があります。<br/><br/>
次のコードは、CyDecalProjector.CreateAndLaunch()メソッドを利用して弾痕を背景に貼り付けるための疑似コードです。この疑似コードでは、CreateAndLaunch()メソッドの引数を使って終了を監視するコールバックを設定しています。<br/>
```C#
// hitPosition    弾丸と背景の衝突点
// hitNormal      衝突した面の法線
// receiverObject デカールを貼り付けるレシーバーオブジェクト
// decalMaterial  デカールマテリアル
void LaunchProjector( 
  Vector3 hitPosition, Vector3 hitNormal, 
  GameObject receiverObject, Material decalMaterial )
{
    var projectorObject = new GameObject("Decal Projector");
    // 法線方向に押し戻した位置にプロジェクターを設置。
    projectorObject.transform.position = hitPosition + hitNormal;
    // プロジェクターの向きは法線の逆向き
    projectorObject.transform.rotation = Quaternion.LookRotation( hitNormal * -1.0f );

    CyDecalProjector.CreateAndLaunch(
                    projectorObj,
                    receiverObject,
                    decalMaterial,
                    /*width=*/0.05f,
                    /*height=*/0.05f,
                    /*depth=*/0.2f
                    /*launchOnAwake*/true,
                    /*onCompletedLaunch*/() => { Destroy(projectorObj); });
}
```



