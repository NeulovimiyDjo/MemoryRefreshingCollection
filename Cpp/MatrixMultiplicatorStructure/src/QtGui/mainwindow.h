#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include "Task.h"
#include <list>

namespace Ui {
class MainWindow;
}

class MainWindow : public QMainWindow
{
  Q_OBJECT
  
public:
  explicit MainWindow(QWidget *parent = 0);
  ~MainWindow();

  int (*event)();
  bool is_alive;
  void setButtonTasksAmount();
  void addButton();

  std::list<Task>* tasks;
  bool tasks_changed = false;
  std::mutex m;
  std::condition_variable cond;
public slots:
  void foo();
  void foo2();
private:
  Ui::MainWindow *ui;
};

#endif // MAINWINDOW_H
