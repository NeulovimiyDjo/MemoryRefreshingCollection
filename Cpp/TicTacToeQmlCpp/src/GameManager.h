#ifndef GAME_MANAGER_H
#define GAME_MANAGER_H

#include <QObject>
#include <QString>
#include <QQmlListProperty>

#include "GameEngine.h"

class SquareManager : public QObject {
  Q_OBJECT
  Q_PROPERTY(QString source READ source NOTIFY sourceChanged)
  Q_PROPERTY(bool isInVictoryChain READ isInVictoryChain NOTIFY isInVictoryChainChanged)

public:
  SquareManager(QObject* parent = nullptr);

  QString source();
  void setSource(const QString& source);
  bool isInVictoryChain();
  void setIsInVictoryChain(const bool source);

signals:
  void sourceChanged();
  void isInVictoryChainChanged();

private:
  QString m_source;
  bool m_is_in_victory_chain;
};


class GameManager : public QObject {
  Q_OBJECT
  Q_PROPERTY(bool isGameOver READ isGameOver NOTIFY isGameOverChanged)
  Q_PROPERTY(QString gameInfo READ gameInfo NOTIFY gameInfoChanged)
  Q_PROPERTY(QQmlListProperty<SquareManager> managers READ managers NOTIFY managersChanged)

public:
  GameManager(QObject* parent = nullptr);

  bool isGameOver();
  QString gameInfo();
  QQmlListProperty<SquareManager> managers();

  Q_INVOKABLE void makeMove(int i, int j);
  Q_INVOKABLE void resetGame();
  Q_INVOKABLE void changeSize(int m, int n, int chain);

signals:
  void isGameOverChanged();
  void gameInfoChanged();
  void managersChanged();

private:
  QList<SquareManager*> m_managers;
  GameEngine m_game_engine;
  bool m_is_game_over;
  QString m_game_info;
  int m_m;
  int m_n;
};

#endif // GAME_MANAGER_H
