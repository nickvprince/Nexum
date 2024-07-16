from flask import Flask

app = Flask(__name__)

@app.route('/server_beat', methods=['post'])
def hello():
    print("Missing heartbeat")
    return "Missing heartbeat"

@app.route('/info', methods=['post'])
def info():
    print("Missing heartbeat")
    return "Missing heartbeat"

if __name__ == '__main__':
    app.run(port=6969)