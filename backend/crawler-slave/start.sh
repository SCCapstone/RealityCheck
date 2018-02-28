rm /tmp/.X99-lock
export DISPLAY=:99
Xvfb :99 -shmem -screen 0 1366x768x16 &
x11vnc -passwd secret -display :99 -N -forever &
python3 slave.py
