from flask import Flask

app = Flask(__name__)

@app.route('/missing_heartbeat', methods=['post'])
def hello():
    print("Missing heartbeat")
    return "Missing heartbeat"

if __name__ == '__main__':
    app.run(port=6969)