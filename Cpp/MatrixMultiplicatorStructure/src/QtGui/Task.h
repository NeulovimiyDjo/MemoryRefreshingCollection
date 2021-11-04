#ifndef TASK_H
#define TASK_H

#include <condition_variable>
#include <iostream>
#include <windows.h>

struct Task {
  std::thread t;
  bool* tasks_changed;
  std::condition_variable* cond;
  std::mutex* m;
  int(*job)();
  bool started_work = false;
  bool finished_work = false;
  //Task(int(*j)()) : t{ nullptr },job { j } {}
  Task(int(*j)(), bool* tasks_changed_in, std::condition_variable* cond_in, std::mutex* m_in)
    : tasks_changed{ tasks_changed_in }, cond{ cond_in }, m{ m_in }, job{ j }
  {
    *tasks_changed = true; // already blocked by a caller, (or could use recursive mutex and block here as well)
    cond->notify_one();
  }
  int operator()() {
    started_work = true;

    //t = std::thread([this] { std::this_thread::sleep_for(std::chrono::milliseconds(2000)); job(); finished_work = true; std::cout << "----FINIESHED WORK----\n"; });

    //std::cout << "-------------/" << GetThreadPriority(t.native_handle()) << "/------------\n";
    //SetThreadPriority(t.native_handle(), THREAD_PRIORITY_BELOW_NORMAL);
    //std::cout << "-------------/" << GetThreadPriority(t.native_handle()) << "/------------\n";

    job();
    finished_work = true;
    {
      std::lock_guard<std::mutex> lck{ *m };
      *tasks_changed = true;
    }
    cond->notify_one();
    std::cout << "----FINIESHED WORK----\n";

    return 0;
  }
  int test() { return 0; }
  //~Task() { if (t && t->joinable()) std::cout << "~Destructor\n"; if (t && t->joinable()) t->join(); if (t) delete t; }
  ~Task() { if (t.joinable()) std::cout << "~Destructor\n"; if (t.joinable()) t.join(); }
  //Task(Task& task) = default;
  Task(Task&& task) = default;
  //Task& operator=(Task& task) = default;
  Task& operator=(Task&& task) = default;
};

#endif // TASK_H