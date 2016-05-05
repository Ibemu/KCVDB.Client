# KCVDB.Client

[![Build status](https://ci.appveyor.com/api/projects/status/hlkqwn71322y07v4?svg=true)](https://ci.appveyor.com/project/kancolleverifyteam/kcvdb-client)

## プロトコル仕様
`kancollevdataapi.azurewebsites.net/api/send`へHTTPSのapplication/x-www-form-urlencodedでPOSTする。艦これAPIを艦これサーバーから受信した順番に検証DBへ送信する。検証DBへの送信が完了するまで、次の艦これAPIを送信してはならない。

POSTする際の引数は以下の通り。

**LoginSessionId** セッションを表すUUID文字列。セッションについては後述。

**AgentId** 送信クライアントを識別するUUID文字列。送信クライアント製作者は検証部へAgentIdの発行を依頼すること。

**Path** 艦これAPIの絶対URL。スキーマ(http)、ホスト(IPアドレスまたはドメイン)も含める。

**RequestValue** 艦これAPIのリクエストボディ。

**ResponseValue** 艦これAPIのレスポンスボディ。なお、`svdata=`等のプレフィックスも削除せずそのまま与える。

**StatusCode** 艦これAPIのレスポンスのステータスコードを表す数値。

**HttpDate** 艦これAPIのレスポンスヘッダーの`Date`フィールドから得られる文字列。

**LocalTime** 送信クライアントが艦これAPIを受信した日時を表す[RFC1123](https://www.ietf.org/rfc/rfc1123.txt)形式の文字列。

### セッション
セッションとは、艦これのAPIが連続していることを保証するためのものである。例として、母港更新→秘書艦の変更を赤城から長門に変更→開発という順で操作した場合で説明する。このとき、提督があらかじめ送信クライアントを立ち上げて起き、母港更新後に送信クライアントを終了し、開発する前に再度送信クライアントを立ち上げた場合に問題がおきる。この場合、送信クライアントは母港更新と開発のAPIだけを受け取り、秘書艦変更のAPIは受け取れない。何も対策をしなかった場合、実際には秘書艦変更後の長門で開発を行ったにもかかわらず、検証DBからは秘書艦変更前の赤城で開発を行ったように見えてしまう。このような事態を防ぐために導入されたのがセッションである。送信クライアントは艦これAPIと共にセッションIDを表すUUID文字列を送信する。送信クライアントは、このセッションIDが等しいAPI同士で、その間にAPIの欠損がなく連続していることを保証しなければならない。先の例の場合は、母港更新のAPIと開発のAPIとでセッションIDに異なるものを指定することで、開発結果を秘書艦不明として処理することができる。

セッションIDは[UUID version 4](https://www.ietf.org/rfc/rfc4122.txt)で生成された一意の文字列である。送信クライアントは以下の場合にこれまでのセッションIDを破棄し、新しい一意のセッションIDを再生成しなければならない。
- 送信クライアントが艦これFlashとプロキシ接続を開始したとき。
- 送信クライアントが受信した艦これAPIを、検証DBへの送信失敗したなどの理由で送信しないまま破棄した場合。

### UUID文字列
UUID文字列は`xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`という形式とする。アルファベットは小文字で、`{}`では囲まない。

### HttpDateとLocalTime
`HttpDate`は艦これサーバーが艦これAPIを送信した日時、`LocalTime`は送信クライアントが艦これAPIを受信した日時として区別し、両方送信する仕様となっている。
