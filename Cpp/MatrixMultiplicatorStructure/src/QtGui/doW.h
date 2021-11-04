#ifndef DO_W_H
#define DO_W_H

#include "mainwindow.h"
#include <functional>
#include <chrono>

struct doW {
  doW(MainWindow& win) :w{ win } {}
  MainWindow& w;
  int operator()()
  {
    while (w.is_alive) {

      //std::this_thread::sleep_for(std::chrono::milliseconds(500));
      //std::cout << "---------CHECK---------\n";

      //if (w.tasks_changed) {

      std::unique_lock<std::mutex> lck{ w.m };
      w.cond.wait(lck, [this] { return w.tasks_changed; });
      std::cout << "---------CHECK LIST---------\n";

      //std::cout << "---ID: " << std::this_thread::get_id() << "---\n";
      //w.cond.wait(lck);
      //std::cout << "before: " << w.tasks.size() << '\n';     
      //w.m.lock();
      for (auto current = w.tasks->begin(); current != w.tasks->end();) {
        if (current->finished_work) {
          std::cout << "---------ERASE---------\n";
          current = w.tasks->erase(current);
          w.setButtonTasksAmount();
        }
        else if (!current->started_work) {
          std::cout << "---------START TASK---------\n";

          //int (Task::*f)()  = &Task::operator();
          //Task* cur = &(*current);
          //(cur->*f)();
          //auto mf = std::mem_fn(f);
          //mf(current);
          //std::thread x(mf,current);


          //std::function<int()> functor{ [] {return 0; } };
          //std::function<int()>* fptr = &functor;
          //std::list<std::function<int()>> lst;
          //lst.emplace_back(functor);
          //auto cr = lst.begin();
          //current->t = move(std::thread(*cr));

          //current->t = new std::thread(std::mem_fn(&Task::operator()), current);

          //current->t = move(std::thread(std::mem_fn(&Task::operator()), current));
          //current->t = move(std::thread([&] { return (*current)(); }));
          current->t = move(std::thread(std::ref(*current)));
          ++current;
        }
        else {
          ++current;
        }
      }

      w.tasks_changed = false;

      //}

      //std::cout << "after: " << w.tasks.size() << '\n';
      //w.m.unlock();
    }

    return 0;
  }
};

#endif // DO_W_H