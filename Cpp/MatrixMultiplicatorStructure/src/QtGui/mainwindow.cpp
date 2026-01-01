#include "mainwindow.h"
#include "ui_mainwindow.h"

MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
    ui->pushButton->setMinimumWidth(200);
    QObject::connect(ui->pushButton, SIGNAL(clicked()), this, SLOT(foo()));

    event = [] {return 0; };
    is_alive = true;
}
//#include <future>
//#include <thread>
#include <iostream>

#include <qboxlayout.h>
void MainWindow::addButton()
{
  QPushButton* button = new QPushButton("STOP ALL",this);
  button->setVisible(true);
  QObject::connect(button, SIGNAL(clicked()), this, SLOT(foo2()));
}

void MainWindow::foo2()
{
  m.lock();
  for (auto current = tasks->begin(); current != tasks->end();) {
    if (current->started_work && !current->finished_work) {
      std::cout << "----------KILL THREAD-----------\n";
      current = tasks->erase(current); // will freeze interface
      setButtonTasksAmount();
    }
    else {
      ++current;
    }
  }
  m.unlock();
}

void MainWindow::setButtonTasksAmount()
{
  ui->pushButton->setText(QString::number(tasks->size()) + " TASKS");
}

void MainWindow::foo()
{
  std::lock_guard<std::mutex> lck{ m };
  if (tasks->size() < 2) {
    tasks->emplace_back(Task{ event , &tasks_changed, &cond, &m});
    ui->pushButton->setText(QString::number(tasks->size())+ " TASKS");
    cond.notify_one();
  }
}

MainWindow::~MainWindow()
{
  delete ui;
}
