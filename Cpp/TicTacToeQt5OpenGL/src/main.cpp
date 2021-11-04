#include "GameWindow.h"

#include <QApplication>
#include <QSurfaceFormat>


int main(int argc, char* argv[]) {
  QApplication app(argc, argv);

  QSurfaceFormat fmt;
  fmt.setProfile(QSurfaceFormat::CoreProfile);
  QSurfaceFormat::setDefaultFormat(fmt);

  GameWindow w;
  w.show();

  return app.exec();
}
