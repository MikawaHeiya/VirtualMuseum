# VirtualMuseum
Virtual Museum 是一款服务于博物馆等公共场所的APP，它提供基于AR稀疏地图的室内导航功能、展示博物馆内文物、使用手势识别操纵UI以及展示的文物等功能。  

![主界面](https://github.com/MikawaHeiya/VirtualMuseum/raw/main/ProjectImages/mainUI.gif)  

## 如何运行此项目
使用Unity Hub打开位于您PC本地的此项目，点击Play按钮开始运行。
如果需要使用手势识别等功能，请将Android手机连接到您的PC上，点击File->Build And Run，在您的Android手机上运行，请注意需要允许应用的摄像头权限请求。
## 功能介绍
### 手势识别
Virtual Museum 中，大多数UI都可以通过手势识别进行操作。
将左手置于Android手机摄像头前方，伸出拇指与食指并将指尖合拢将输入UI光标向下一个移动的指令，左手由非握拳状态转为握拳状态将输入“确定”指令。
默认情况下“确定”指令所识别的手势为握拳，可以在设置界面更改。  

![设置](https://github.com/MikawaHeiya/VirtualMuseum/raw/main/ProjectImages/config_scene.jpg)  

### 通过二维码生成并展示3D物体
用户可以通过扫描二维码的方式在Android手机上更直观地欣赏博物馆内的文物，请注意此过程需要网络连接。

![扫码](https://github.com/MikawaHeiya/VirtualMuseum/raw/main/ProjectImages/qrcode.gif)  

生成的3D物体同样可以进行手势识别的交互，将左手置于Android手机摄像机前，食指与拇指轻触以切换“旋转”或“缩放”模式，保持握拳并旋转调整拳头角度以旋转或缩放3D物体。  

![3D](https://github.com/MikawaHeiya/VirtualMuseum/raw/main/ProjectImages/box_demo.gif)

### 登录
Virtual Museum 提供用户系统，并使用邮箱验证的方式登陆或注册。
在主界面点击右上角的登录按钮，弹出的对话框中在邮箱输入栏中输入您的邮箱账号，点击下方的“发送”按钮，在验证码输入框中输入您收到的验证码，点击“登录”按钮即可。  

![登录](https://github.com/MikawaHeiya/VirtualMuseum/raw/main/ProjectImages/login_demo.gif)  

