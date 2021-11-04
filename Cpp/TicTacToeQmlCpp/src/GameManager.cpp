#include "GameManager.h"

SquareManager::SquareManager(QObject* parent) : QObject(parent) {
  m_source = ":/img/clear.png";
  m_is_in_victory_chain = false;
}

QString SquareManager::source() {
  return m_source;
}

void SquareManager::setSource(const QString& source) {
  m_source = source;
  emit sourceChanged();
}

bool SquareManager::isInVictoryChain() {
  return m_is_in_victory_chain;
}

void SquareManager::setIsInVictoryChain(const bool is_in_victory_chain) {
  m_is_in_victory_chain = is_in_victory_chain;
  emit isInVictoryChainChanged();
}


//------------


GameManager::GameManager(QObject* parent) : QObject(parent) {
  m_m = 3;
  m_n = 3;

  for (int i = 0; i < m_m*m_n; ++i)
    m_managers.append(new SquareManager(this));

  m_is_game_over = false;
  m_game_info = "Moves made: 0";
}

bool GameManager::isGameOver() {
  return m_is_game_over;
}

QString GameManager::gameInfo() {
  return m_game_info;
}

QQmlListProperty<SquareManager> GameManager::managers() {
  return QQmlListProperty<SquareManager>(this, m_managers);
}

void GameManager::makeMove(int i, int j) {
  if (m_game_engine.makeMove(i, j)) {
    GameEngine::SquareStatus square_status = m_game_engine.getSquareStatus(i, j);
    int moves_count = m_game_engine.getMovesCount();

    if (square_status == GameEngine::SquareStatus::Cross)
      m_managers[i*m_n+j]->setSource(":/img/cross.png");
    else if (square_status == GameEngine::SquareStatus::Nought)
      m_managers[i*m_n+j]->setSource(":/img/nought.png");

    GameEngine::GameStatus game_status = m_game_engine.getGameStatus();
    if (game_status != GameEngine::GameStatus::InProgress) {
      std::vector<int> victory_squares = m_game_engine.getVictorySquares();
      switch (game_status) {
      case GameEngine::GameStatus::CrossesWon:
        for (int position : victory_squares)
          m_managers[position]->setIsInVictoryChain(true);
        m_game_info = QString("Moves made: %1\nCrosses Won").arg(moves_count);
        break;
      case GameEngine::GameStatus::NoughtsWon:
        for (int position : victory_squares)
          m_managers[position]->setIsInVictoryChain(true);
        m_game_info = QString("Moves made: %1\nNoughts Won").arg(moves_count);
        break;
      case GameEngine::GameStatus::Draw:
        m_game_info = QString("Moves made: %1\nDraw").arg(moves_count);
        break;
      }

      m_is_game_over = true;
      emit isGameOverChanged();
    }
     else
        m_game_info = QString("Moves made: %1").arg(moves_count);

     emit gameInfoChanged();
  }
}

void GameManager::resetGame() {
  m_game_engine.resetGame();

  for (auto manager : m_managers) {
    manager->setSource(":/img/clear.png");
    manager->setIsInVictoryChain(false);
  }

  m_game_info = "Moves made: 0";
  emit gameInfoChanged();

  if (m_is_game_over) {
    m_is_game_over = false;
    emit isGameOverChanged();
  }
}

void GameManager::changeSize(int m, int n, int chain) {
  if (m_game_engine.changeSize(m, n, chain)) {
    for (auto manager : m_managers)
      delete manager;

    m_managers.clear();

    m_m = m;
    m_n = n;

    for (int i = 0; i < m_m*m_n; ++i)
      m_managers.append(new SquareManager(this));

    m_game_info = "Moves made: 0";
    emit gameInfoChanged();

    if (m_is_game_over) {
      m_is_game_over = false;
      emit isGameOverChanged();
    }
  }
}
