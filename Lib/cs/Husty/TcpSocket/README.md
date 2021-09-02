# Husty.TcpSocket
異なるプロセス間において、IPとポートを頼りにバイナリ情報の受け渡しをします。  
各クラスの階層構造は  
```
ITcpSocket ─ TcpSocketBase ┬ Server  
                       　　└ Client  
```
となっており、共通インターフェースで受け側、送る側の記述ができます。  
送受信するものはジェネリック型で、配列はカンマ区切りにして配列のまま扱えます。   
個人的にOpenCvSharpを使うことが多いのでMatもそのまま扱えるようにしました。Husty.TcpSocket.MatExtensionsをインストールすれば拡張機能としてSendImage, ReceiveImageが使えるようになります。
