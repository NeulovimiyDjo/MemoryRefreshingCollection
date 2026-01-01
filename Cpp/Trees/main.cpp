#include <cstdlib>
#include <iostream>
#include <queue>
#include <utility>

namespace Trees {
  class Set {
  public:
    ~Set() { if (root) delete root; }

    bool contains(int value) {
      Node* n = find(value, root);
      if (n)
        return true;
      else
        return false;
    }

    void insert(int value) { insert(value, root); }

    void remove(int value) { remove(value, root); }


    void print();

  private:
    class Node {
    public:
      ~Node() {
        if (left) delete left;
        if (right) delete right;
      }

      int val;

      Node* left = nullptr;
      Node* right = nullptr;
    };


    Node* find(int value, Node* node);
    std::pair<Node*, Node*> findMin(Node* node, Node* parent);
    void insert(int value, Node*& node);
    void remove(int value, Node*& node);


    Node* root = nullptr;
  };




  void Set::print() {
    std::queue<std::pair<Node*, int>> q;
    q.push(std::make_pair(root, 0));

    int currentDepth = 0;
    while (!q.empty()) {
      auto el = q.front();
      q.pop();

      Node* n = el.first;
      if (n == nullptr) continue;

      int depth = el.second;
      if (depth > currentDepth) {
        currentDepth = depth;
        std::cout << '\n';
      }

      std::cout << n->val << " ";

      q.push(std::make_pair(n->left, depth + 1));
      q.push(std::make_pair(n->right, depth + 1));
    }
  }


  Set::Node* Set::find(int value, Node* node) {
    if (node == nullptr) return nullptr;

    if (value == node->val) return node;

    if (value < node->val)
      return find(value, node->left);
    else if (value > node->val)
      return find(value, node->right);
  }


  std::pair<Set::Node*, Set::Node*> Set::findMin(Node* node, Node* parent) {
    if (node->left) {
      return findMin(node->left, node);
    } else {
      return std::make_pair(node, parent);
    }
  }


  void Set::insert(int value, Node*& node) {
    if (!node) {
      node = new Node();
      node->val = value;
    } else {
      if (value == node->val) return;

      if (value < node->val)
        insert(value, node->left);
      else if (value > node->val)
        insert(value, node->right);
    }
  }

  void Set::remove(int value, Node*& node) {
    if (node) {
      if (value == node->val) {
        if (!node->left && !node->right) { // 0 children
          delete node;
          node = nullptr;
        } else if (!node->left || !node->right) { // 1 child
          Node* delNode = node;

          if (node->left) {
            node = delNode->left;
            delNode->left = nullptr;
          } else {
            node = delNode->right;
            delNode->right = nullptr;
          }
          
          delete delNode;
        } else { // two children
          auto minAndParent = findMin(node->right, node);
          Node* successor = minAndParent.first;
          Node* successorParent = minAndParent.second;

          node->val = successor->val;

          successorParent->left = nullptr;
          remove(successor->val, successor);
        }

        return;
      }

      if (value < node->val)
        remove(value, node->left);
      else if (value > node->val)
        remove(value, node->right);
    }
  }
} // namespace Trees



void test1() {
  Trees::Set set;
  set.insert(11);

  set.insert(17);
  set.insert(22);
  set.insert(15);

  set.insert(5);


  //set.remove(11);
  //set.remove(22);

  set.print();
}

void test2() {
  Trees::Set set;
  set.insert(11);

  set.insert(17);
  set.insert(22);
  set.insert(15);

  set.insert(5);
  set.insert(3);
  set.insert(2);
  set.insert(24);
  set.insert(26);


  set.remove(11);
  set.remove(24);
  set.insert(21);


  set.print();
}


int main(int argc, char* argv[]) {
  test2();
  

  std::cin.get();
  return EXIT_SUCCESS;
}