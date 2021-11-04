#include <QApplication>
#include <QQmlApplicationEngine>
#include <QIcon>

#include "GameManager.h"
#include "ImageButton.h"

int main(int argc, char* argv[])
{
  // i have flickering on resize on my desktop without this for some reason
  QApplication::setAttribute(Qt::AA_UseSoftwareOpenGL);
  QApplication app(argc, argv);
  app.setWindowIcon(QIcon(":/img/icon.png"));

  qmlRegisterType<SquareManager>("SquareManager", 1, 0, "SquareManager");
  qmlRegisterType<GameManager>("GameManager", 1, 0, "GameManager");
  qmlRegisterType<ImageButton>("ImageButton", 1, 0, "ImageButton");

  QQmlApplicationEngine engine;
  engine.load("qrc:/qml/gui.qml");

  return app.exec();
}
