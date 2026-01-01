#ifndef RUNNER_H
#define RUNNER_H

#ifdef PROJECT_EXPORTS
  #define IMPORT_EXPORT __declspec(dllexport)
#else
  #define IMPORT_EXPORT __declspec(dllimport)
#endif

int IMPORT_EXPORT runGui(int argc, char* argv[]);

int IMPORT_EXPORT setEvent(int());

#endif