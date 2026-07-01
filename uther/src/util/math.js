export default {
  /**
 * 精确加法
 * @param {number} a 
 * @param {number} b 
 * @returns {number}
 */
  add: (a, b) => {
    const aDecimals = (a.toString().split('.')[1] || '').length
    const bDecimals = (b.toString().split('.')[1] || '').length
    const maxDecimals = Math.max(aDecimals, bDecimals)
    const multiplier = Math.pow(10, maxDecimals)

    return (Math.round(a * multiplier) + Math.round(b * multiplier)) / multiplier
  },

  /**
   * 精确减法
   * @param {number} a 
   * @param {number} b 
   * @returns {number}
   */
  subtract: (a, b) => {
    return add(a, -b)
  },

  /**
   * 精确乘法
   * @param {number} a 
   * @param {number} b 
   * @returns {number}
   */
  multiply: (a, b) => {
    const aDecimals = (a.toString().split('.')[1] || '').length
    const bDecimals = (b.toString().split('.')[1] || '').length
    const multiplier = Math.pow(10, aDecimals + bDecimals)

    return (Math.round(a * Math.pow(10, aDecimals)) * Math.round(b * Math.pow(10, bDecimals))) / multiplier
  },

  /**
   * 精确除法
   * @param {number} a 
   * @param {number} b 
   * @returns {number}
   */
  divide: (a, b) => {
    const aDecimals = (a.toString().split('.')[1] || '').length
    const bDecimals = (b.toString().split('.')[1] || '').length
    const multiplier = Math.pow(10, Math.max(aDecimals, bDecimals))

    return (Math.round(a * multiplier) / Math.round(b * multiplier))
  }

}
