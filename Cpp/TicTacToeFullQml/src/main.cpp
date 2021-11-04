#include <QApplication>
#include <QQmlApplicationEngine>

int main(int argc, char* argv[])
{
  QApplication app(argc, argv);

  QQmlApplicationEngine engine;
  engine.load(QUrl(QStringLiteral("qrc:/qml/gui.qml")));

  return app.exec();
}
