#include "runner.h"

#include "mainwindow.h"
#include <QApplication>

#include "doW.h"

int (*gevent)();

int IMPORT_EXPORT setEvent(int f())
{
  gevent = f;
  return 0;
}


int runGui(int argc, char* argv[])
{
  QApplication a(argc, argv);
  MainWindow w;
  w.event = gevent;
  w.show();

  std::list<Task> tasks;
  w.tasks = &tasks;

  std::thread worker(doW{ w });
  
  //std::cout << "-------------/" << GetThreadPriority(GetCurrentThread()) << "/------------\n";

  w.addButton();
  int res = a.exec();

  w.is_alive = false;
  worker.join();
  return res;
}