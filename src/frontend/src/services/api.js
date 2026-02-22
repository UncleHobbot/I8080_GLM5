const API_BASE = 'http://localhost:5000/api';

export const emulatorApi = {
  async createSession() {
    const response = await fetch(`${API_BASE}/emulator/session`, { method: 'POST' });
    return response.json();
  },

  async getSessionStatus(sessionId) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}`);
    return response.json();
  },

  async sendCommand(sessionId, command) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}/command`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ command })
    });
    return response.json();
  },

  async sendInput(sessionId, input) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}/input`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ input })
    });
    return response.ok;
  },

  async loadProgram(sessionId, program, address = 0x0100) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}/load`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ program: Array.from(program), address })
    });
    return response.ok;
  },

  async runProgram(sessionId, startAddress = 0x0100, maxCycles = 100000) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}/run`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ startAddress, maxCycles })
    });
    return response.json();
  },

  async stepProgram(sessionId) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}/step`, {
      method: 'POST'
    });
    return response.json();
  },

  async resetCpu(sessionId) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}/reset`, {
      method: 'POST'
    });
    return response.ok;
  },

  async dumpMemory(sessionId, address = 0, length = 256) {
    const response = await fetch(
      `${API_BASE}/emulator/session/${sessionId}/memory?address=${address}&length=${length}`
    );
    return response.json();
  },

  async deleteSession(sessionId) {
    const response = await fetch(`${API_BASE}/emulator/session/${sessionId}`, {
      method: 'DELETE'
    });
    return response.ok;
  }
};
