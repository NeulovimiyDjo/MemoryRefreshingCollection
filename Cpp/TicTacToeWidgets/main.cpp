#include <QApplication>
#include <QWidget>
#include <QIcon>
#include "GameWindow.h"

int main(int argc, char* argv[])
{
  QApplication app(argc, argv);

  GameWindow window;
  window.resize(640, 480);
  window.move(100, 100);
  window.setWindowTitle("Title");
  window.setWindowIcon(QIcon(":/icon.png"));

  window.show();

  return app.exec();
}